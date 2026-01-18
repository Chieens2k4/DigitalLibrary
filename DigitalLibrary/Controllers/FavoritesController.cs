using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using System.Security.Claims;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public FavoritesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // GET: api/Favorites
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetFavoriteDocuments()
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

                var favoriteDocuments = await _context.FavDocs
                    .Include(f => f.Document)
                        .ThenInclude(d => d.Category)
                    .Include(f => f.Document)
                        .ThenInclude(d => d.ViewLogs)
                    .Include(f => f.Document)
                        .ThenInclude(d => d.DownloadLogs)
                    .Include(f => f.Document)
                        .ThenInclude(d => d.Reviews)
                    .Where(f => f.UserId == userId.Value)
                    .Select(f => f.Document)
                    .ToListAsync();

                var documentDtos = favoriteDocuments.Select(d => new DocumentDto
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
                    IsFavorite = true
                }).ToList();

                return Ok(new ApiResponse<List<DocumentDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách yêu thích thành công",
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

        // POST: api/Favorites/{documentId}
        [HttpPost("{documentId}")]
        public async Task<ActionResult<ApiResponse<object>>> AddToFavorites(int documentId)
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

                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var existingFav = await _context.FavDocs
                    .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.DocumentId == documentId);

                if (existingFav != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Tài liệu đã có trong danh sách yêu thích"
                    });
                }

                var favDoc = new FavDoc
                {
                    UserId = userId.Value,
                    DocumentId = documentId
                };

                _context.FavDocs.Add(favDoc);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Thêm vào yêu thích thành công"
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

        // DELETE: api/Favorites/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveFromFavorites(int documentId)
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

                var favDoc = await _context.FavDocs
                    .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.DocumentId == documentId);

                if (favDoc == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy trong danh sách yêu thích"
                    });
                }

                _context.FavDocs.Remove(favDoc);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa khỏi yêu thích thành công"
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

        // GET: api/Favorites/check/{documentId}
        [HttpGet("check/{documentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckIsFavorite(int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var isFavorite = await _context.FavDocs
                    .AnyAsync(f => f.UserId == userId.Value && f.DocumentId == documentId);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Kiểm tra thành công",
                    Data = isFavorite
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}