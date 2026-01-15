using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public CategoriesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Documents)
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

        // GET: api/Categories/{id}
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
    }
}