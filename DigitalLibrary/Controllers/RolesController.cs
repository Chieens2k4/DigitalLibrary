using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public RolesController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/Roles - Public (để hiển thị trong registration form, etc.)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.Id)
                    .ToListAsync();

                var roleDtos = roles.Select(r => new RoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? "",
                    Description = r.Description
                }).ToList();

                return Ok(new ApiResponse<List<RoleDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách vai trò thành công",
                    Data = roleDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<RoleDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/Roles/{id} - Public
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleDto>>> GetRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound(new ApiResponse<RoleDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy vai trò"
                    });
                }

                var roleDto = new RoleDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? "",
                    Description = role.Description
                };

                return Ok(new ApiResponse<RoleDto>
                {
                    Success = true,
                    Message = "Lấy thông tin vai trò thành công",
                    Data = roleDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}