using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Authorization;

namespace DigitalLibrary.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ReviewsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        [RequirePermission("Review", "View")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ReviewDto>>>> GetReviews(
            [FromQuery] int? documentId,
            [FromQuery] int? userId,
            [FromQuery] int? minRating,
            [FromQuery] int? maxRating,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Document)
                    .AsQueryable();

                if (documentId.HasValue)
                    query = query.Where(r => r.DocumentId == documentId.Value);

                if (userId.HasValue)
                    query = query.Where(r => r.UserId == userId.Value);

                if (minRating.HasValue)
                    query = query.Where(r => r.StarRate >= minRating.Value);

                if (maxRating.HasValue)
                    query = query.Where(r => r.StarRate <= maxRating.Value);

                var totalCount = await query.CountAsync();

                var reviews = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var reviewDtos = reviews.Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    UserName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                    DocumentId = r.DocumentId,
                    Comment = r.Comment,
                    StarRate = r.StarRate,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var result = new PagedResultDto<ReviewDto>
                {
                    Items = reviewDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(new ApiResponse<PagedResultDto<ReviewDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách đánh giá thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<ReviewDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Review", "View")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewDetailDto>>> GetReview(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Document)
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    return NotFound(new ApiResponse<ReviewDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá"
                    });
                }

                var reviewDetail = new ReviewDetailDto
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId,
                    UserEmail = review.User?.Email ?? "",
                    UserName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                    DocumentId = review.DocumentId,
                    DocumentTitle = review.Document?.Title ?? "",
                    Comment = review.Comment,
                    StarRate = review.StarRate,
                    CreatedAt = review.CreatedAt
                };

                return Ok(new ApiResponse<ReviewDetailDto>
                {
                    Success = true,
                    Message = "Lấy thông tin đánh giá thành công",
                    Data = reviewDetail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ReviewDetailDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Review", "Moderate")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá"
                    });
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa đánh giá thành công"
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

        [RequirePermission("Review", "Moderate")]
        [HttpDelete("bulk-delete")]
        public async Task<ActionResult<ApiResponse<object>>> BulkDeleteReviews([FromBody] List<int> reviewIds)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => reviewIds.Contains(r.ReviewId))
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá nào"
                    });
                }

                _context.Reviews.RemoveRange(reviews);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Xóa {reviews.Count} đánh giá thành công"
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

        [RequirePermission("Review", "View")]
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<ReviewStatisticsDto>>> GetStatistics()
        {
            try
            {
                var totalReviews = await _context.Reviews.CountAsync();
                var avgRating = totalReviews > 0
                    ? await _context.Reviews.AverageAsync(r => r.StarRate)
                    : 0;

                var ratingDistribution = await _context.Reviews
                    .GroupBy(r => r.StarRate)
                    .Select(g => new RatingDistributionDto
                    {
                        StarRate = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.StarRate)
                    .ToListAsync();

                var statistics = new ReviewStatisticsDto
                {
                    TotalReviews = totalReviews,
                    AverageRating = avgRating,
                    RatingDistribution = ratingDistribution,
                    ReviewsThisMonth = await _context.Reviews
                        .CountAsync(r => r.CreatedAt >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                };

                return Ok(new ApiResponse<ReviewStatisticsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê đánh giá thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ReviewStatisticsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}