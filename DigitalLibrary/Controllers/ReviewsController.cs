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
    public class ReviewsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public ReviewsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // GET: api/Reviews/document/{documentId} - Public
        [HttpGet("document/{documentId}")]
        public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetDocumentReviews(int documentId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Where(r => r.DocumentId == documentId)
                    .OrderByDescending(r => r.CreatedAt)
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

                return Ok(new ApiResponse<List<ReviewDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách đánh giá thành công",
                    Data = reviewDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ReviewDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/Reviews - Yêu cầu permission Review:Create
        [RequirePermission("Review", "Create")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview(CreateReviewDto createReviewDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                // Kiểm tra tài liệu có tồn tại
                var document = await _context.Documents.FindAsync(createReviewDto.DocumentId);
                if (document == null)
                {
                    return NotFound(new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                // Kiểm tra user đã đánh giá tài liệu này chưa
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId.Value && r.DocumentId == createReviewDto.DocumentId);

                if (existingReview != null)
                {
                    return BadRequest(new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Bạn đã đánh giá tài liệu này rồi"
                    });
                }

                var review = new Review
                {
                    UserId = userId.Value,
                    DocumentId = createReviewDto.DocumentId,
                    Comment = createReviewDto.Comment,
                    StarRate = createReviewDto.StarRate,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                await _context.Entry(review).Reference(r => r.User).LoadAsync();

                var reviewDto = new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId,
                    UserName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                    DocumentId = review.DocumentId,
                    Comment = review.Comment,
                    StarRate = review.StarRate,
                    CreatedAt = review.CreatedAt
                };

                return CreatedAtAction(nameof(GetDocumentReviews),
                    new { documentId = review.DocumentId },
                    new ApiResponse<ReviewDto>
                    {
                        Success = true,
                        Message = "Tạo đánh giá thành công",
                        Data = reviewDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/Reviews/{id} - Yêu cầu permission Review:Edit + phải là owner
        [RequirePermission("Review", "Edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateReview(int id, CreateReviewDto updateReviewDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Vui lòng đăng nhập"
                    });
                }

                var review = await _context.Reviews
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    return NotFound(new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá"
                    });
                }

                // Chỉ cho phép user chỉnh sửa đánh giá của chính mình
                if (review.UserId != userId.Value)
                {
                    return Forbid();
                }

                review.Comment = updateReviewDto.Comment;
                review.StarRate = updateReviewDto.StarRate;

                await _context.SaveChangesAsync();

                var reviewDto = new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId,
                    UserName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                    DocumentId = review.DocumentId,
                    Comment = review.Comment,
                    StarRate = review.StarRate,
                    CreatedAt = review.CreatedAt
                };

                return Ok(new ApiResponse<ReviewDto>
                {
                    Success = true,
                    Message = "Cập nhật đánh giá thành công",
                    Data = reviewDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // DELETE: api/Reviews/{id} - Yêu cầu permission Review:Delete + phải là owner
        [RequirePermission("Review", "Delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteReview(int id)
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

                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy đánh giá"
                    });
                }

                // Chỉ cho phép user xóa đánh giá của chính mình
                if (review.UserId != userId.Value)
                {
                    return Forbid();
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
    }
}