using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using DigitalLibrary.Authorization;
using System.Security.Claims;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DocumentsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // GET: api/Documents - Public, không cần permission
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<DocumentDto>>>> GetDocuments(
            [FromQuery] DocumentSearchDto searchDto)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .AsQueryable();

                // Lọc theo từ khóa tìm kiếm
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var searchTerm = searchDto.SearchTerm.ToLower();
                    query = query.Where(d =>
                        d.Title.ToLower().Contains(searchTerm) ||
                        (d.Abstract != null && d.Abstract.ToLower().Contains(searchTerm)) ||
                        (d.AuthorName != null && d.AuthorName.ToLower().Contains(searchTerm)));
                }

                if (searchDto.CategoryId.HasValue)
                {
                    query = query.Where(d => d.CategoryId == searchDto.CategoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.AuthorName))
                {
                    query = query.Where(d => d.AuthorName != null &&
                        d.AuthorName.ToLower().Contains(searchDto.AuthorName.ToLower()));
                }

                if (searchDto.PublishYear.HasValue)
                {
                    query = query.Where(d => d.PublishYear == searchDto.PublishYear.Value);
                }

                // Sắp xếp
                query = searchDto.SortBy?.ToLower() switch
                {
                    "title" => searchDto.SortDescending
                        ? query.OrderByDescending(d => d.Title)
                        : query.OrderBy(d => d.Title),
                    "date" => searchDto.SortDescending
                        ? query.OrderByDescending(d => d.DateUploaded)
                        : query.OrderBy(d => d.DateUploaded),
                    "views" => searchDto.SortDescending
                        ? query.OrderByDescending(d => d.ViewLogs.Count)
                        : query.OrderBy(d => d.ViewLogs.Count),
                    "downloads" => searchDto.SortDescending
                        ? query.OrderByDescending(d => d.DownloadLogs.Count)
                        : query.OrderBy(d => d.DownloadLogs.Count),
                    "rating" => searchDto.SortDescending
                        ? query.OrderByDescending(d => d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0)
                        : query.OrderBy(d => d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0),
                    _ => query.OrderByDescending(d => d.DateUploaded)
                };

                var totalCount = await query.CountAsync();

                var documents = await query
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var userId = GetCurrentUserId();
                var favoriteDocIds = userId.HasValue
                    ? await _context.FavDocs
                        .Where(f => f.UserId == userId.Value)
                        .Select(f => f.DocumentId)
                        .ToListAsync()
                    : new List<int>();

                var documentDtos = documents.Select(d => new DocumentDto
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

                var result = new PagedResultDto<DocumentDto>
                {
                    Items = documentDtos,
                    TotalCount = totalCount,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize
                };

                return Ok(new ApiResponse<PagedResultDto<DocumentDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách tài liệu thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<DocumentDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/Documents/{id} - Public, không cần permission
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDto>>> GetDocument(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .FirstOrDefaultAsync(d => d.DocumentId == id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<DocumentDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var userId = GetCurrentUserId();
                var isFavorite = userId.HasValue && await _context.FavDocs
                    .AnyAsync(f => f.UserId == userId.Value && f.DocumentId == id);

                var documentDto = new DocumentDto
                {
                    DocumentId = document.DocumentId,
                    Title = document.Title,
                    CategoryId = document.CategoryId,
                    CategoryName = document.Category?.CategoryName,
                    Abstract = document.Abstract,
                    AuthorName = document.AuthorName,
                    PublishYear = document.PublishYear,
                    FilePath = document.FilePath,
                    AccessLevel = document.AccessLevel,
                    DateUploaded = document.DateUploaded,
                    ViewCount = document.ViewLogs.Count,
                    DownloadCount = document.DownloadLogs.Count,
                    AverageRating = document.Reviews.Any() ? document.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = document.Reviews.Count,
                    IsFavorite = isFavorite
                };

                return Ok(new ApiResponse<DocumentDto>
                {
                    Success = true,
                    Message = "Lấy thông tin tài liệu thành công",
                    Data = documentDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/Documents/{id}/view - Chỉ cần đăng nhập
        [Authorize]
        [HttpPost("{id}/view")]
        public async Task<ActionResult<ApiResponse<object>>> LogView(int id)
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

                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var viewLog = new ViewLog
                {
                    UserId = userId.Value,
                    DocumentId = id,
                    Time = DateTime.Now
                };

                _context.ViewLogs.Add(viewLog);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Ghi nhận lượt xem thành công"
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

        // POST: api/Documents/{id}/download - Yêu cầu permission Document:Download
        [RequirePermission("Document", "Download")]
        [HttpPost("{id}/download")]
        public async Task<ActionResult<ApiResponse<object>>> LogDownload(int id)
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

                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var downloadLog = new DownloadLog
                {
                    UserId = userId.Value,
                    DocumentId = id,
                    Time = DateTime.Now
                };

                _context.DownloadLogs.Add(downloadLog);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Ghi nhận lượt tải thành công",
                    Data = new { FilePath = document.FilePath }
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

        // GET: api/Documents/popular - Public
        [HttpGet("popular")]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetPopularDocuments([FromQuery] int count = 10)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .OrderByDescending(d => d.ViewLogs.Count + d.DownloadLogs.Count)
                    .Take(count)
                    .ToListAsync();

                var userId = GetCurrentUserId();
                var favoriteDocIds = userId.HasValue
                    ? await _context.FavDocs
                        .Where(f => f.UserId == userId.Value)
                        .Select(f => f.DocumentId)
                        .ToListAsync()
                    : new List<int>();

                var documentDtos = documents.Select(d => new DocumentDto
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
                    Message = "Lấy danh sách tài liệu phổ biến thành công",
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

        // GET: api/Documents/recent - Public
        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetRecentDocuments([FromQuery] int count = 10)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .OrderByDescending(d => d.DateUploaded)
                    .Take(count)
                    .ToListAsync();

                var userId = GetCurrentUserId();
                var favoriteDocIds = userId.HasValue
                    ? await _context.FavDocs
                        .Where(f => f.UserId == userId.Value)
                        .Select(f => f.DocumentId)
                        .ToListAsync()
                    : new List<int>();

                var documentDtos = documents.Select(d => new DocumentDto
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
                    Message = "Lấy danh sách tài liệu mới thành công",
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