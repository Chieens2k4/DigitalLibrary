using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using DigitalLibrary.Authorization;

namespace DigitalLibrary.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public CategoriesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        [RequirePermission("Category", "View")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Documents)
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    DocumentCount = c.Documents.Count
                }).ToList();

                return Ok(new ApiResponse<List<CategoryDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách danh mục thành công",
                    Data = categoryDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Category", "View")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    });
                }

                var categoryDto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    DocumentCount = category.Documents.Count
                };

                return Ok(new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Message = "Lấy thông tin danh mục thành công",
                    Data = categoryDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Category", "Create")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                if (await _context.Categories.AnyAsync(c => c.CategoryName == createDto.CategoryName))
                {
                    return BadRequest(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Tên danh mục đã tồn tại"
                    });
                }

                var category = new Category
                {
                    CategoryName = createDto.CategoryName
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    DocumentCount = 0
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId },
                    new ApiResponse<CategoryDto>
                    {
                        Success = true,
                        Message = "Tạo danh mục thành công",
                        Data = categoryDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Category", "Edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    });
                }

                if (await _context.Categories.AnyAsync(c => c.CategoryName == updateDto.CategoryName && c.CategoryId != id))
                {
                    return BadRequest(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Tên danh mục đã tồn tại"
                    });
                }

                category.CategoryName = updateDto.CategoryName;
                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    DocumentCount = category.Documents.Count
                };

                return Ok(new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Message = "Cập nhật danh mục thành công",
                    Data = categoryDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Category", "Delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục"
                    });
                }

                if (category.Documents.Any())
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Không thể xóa danh mục có {category.Documents.Count} tài liệu. Vui lòng xóa hoặc chuyển tài liệu sang danh mục khác trước."
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa danh mục thành công"
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