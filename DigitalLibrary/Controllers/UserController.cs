using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Services;
using System.Security.Claims;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserController(DigitalLibraryContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // GET: api/User/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId.Value);

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var profileDto = new UserProfileDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    RoleName = user.Role?.RoleName ?? "",
                    CreatedAt = user.CreatedAt
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Lấy thông tin cá nhân thành công",
                    Data = profileDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/User/profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(UpdateProfileDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId.Value);

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.DateOfBirth = updateDto.DateOfBirth;
                user.Gender = updateDto.Gender;

                await _context.SaveChangesAsync();

                var profileDto = new UserProfileDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    RoleName = user.Role?.RoleName ?? "",
                    CreatedAt = user.CreatedAt
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công",
                    Data = profileDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/User/change-password
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Kiểm tra mật khẩu cũ
                if (!_passwordHasher.VerifyPassword(changePasswordDto.OldPassword, user.PasswordHash))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Mật khẩu cũ không chính xác"
                    });
                }

                // Cập nhật mật khẩu mới
                user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/User/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<UserStatisticsDto>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var statistics = new UserStatisticsDto
                {
                    TotalViewed = await _context.ViewLogs.CountAsync(v => v.UserId == userId.Value),
                    TotalDownloaded = await _context.DownloadLogs.CountAsync(d => d.UserId == userId.Value),
                    TotalFavorites = await _context.FavDocs.CountAsync(f => f.UserId == userId.Value),
                    TotalReviews = await _context.Reviews.CountAsync(r => r.UserId == userId.Value)
                };

                return Ok(new ApiResponse<UserStatisticsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserStatisticsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/User/view-history
        [HttpGet("view-history")]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetViewHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<List<DocumentDto>>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var viewedDocuments = await _context.ViewLogs
                    .Include(v => v.Document)
                        .ThenInclude(d => d.Category)
                    .Include(v => v.Document)
                        .ThenInclude(d => d.ViewLogs)
                    .Include(v => v.Document)
                        .ThenInclude(d => d.DownloadLogs)
                    .Include(v => v.Document)
                        .ThenInclude(d => d.Reviews)
                    .Where(v => v.UserId == userId.Value)
                    .OrderByDescending(v => v.Time)
                    .Select(v => v.Document)
                    .Distinct()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var favoriteDocIds = await _context.FavDocs
                    .Where(f => f.UserId == userId.Value)
                    .Select(f => f.DocumentId)
                    .ToListAsync();

                var documentDtos = viewedDocuments.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category?.CategoryName,
                    Abstract = d.Abstract,
                    AuthorName = d.AuthorName,
                    PublishYear = d.PublishYear,
                    FilePath = d.FilePath,
                    AccessLevel = d.AccessLevel,
                    DateUploaded = d.DateUploaded,
                    ViewCount = d.ViewLogs.Count,
                    DownloadCount = d.DownloadLogs.Count,
                    AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = d.Reviews.Count,
                    IsFavorite = favoriteDocIds.Contains(d.DocumentId)
                }).ToList();

                return Ok(new ApiResponse<List<DocumentDto>>
                {
                    Success = true,
                    Message = "Lấy lịch sử xem thành công",
                    Data = documentDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/User/download-history
        [HttpGet("download-history")]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetDownloadHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<List<DocumentDto>>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var downloadedDocuments = await _context.DownloadLogs
                    .Include(d => d.Document)
                        .ThenInclude(doc => doc.Category)
                    .Include(d => d.Document)
                        .ThenInclude(doc => doc.ViewLogs)
                    .Include(d => d.Document)
                        .ThenInclude(doc => doc.DownloadLogs)
                    .Include(d => d.Document)
                        .ThenInclude(doc => doc.Reviews)
                    .Where(d => d.UserId == userId.Value)
                    .OrderByDescending(d => d.Time)
                    .Select(d => d.Document)
                    .Distinct()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var favoriteDocIds = await _context.FavDocs
                    .Where(f => f.UserId == userId.Value)
                    .Select(f => f.DocumentId)
                    .ToListAsync();

                var documentDtos = downloadedDocuments.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category?.CategoryName,
                    Abstract = d.Abstract,
                    AuthorName = d.AuthorName,
                    PublishYear = d.PublishYear,
                    FilePath = d.FilePath,
                    AccessLevel = d.AccessLevel,
                    DateUploaded = d.DateUploaded,
                    ViewCount = d.ViewLogs.Count,
                    DownloadCount = d.DownloadLogs.Count,
                    AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = d.Reviews.Count,
                    IsFavorite = favoriteDocIds.Contains(d.DocumentId)
                }).ToList();

                return Ok(new ApiResponse<List<DocumentDto>>
                {
                    Success = true,
                    Message = "Lấy lịch sử tải thành công",
                    Data = documentDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}