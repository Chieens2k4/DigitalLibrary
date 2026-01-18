using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using DigitalLibrary.Authorization;
using System.Security.Claims;

namespace DigitalLibrary.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(
            DigitalLibraryContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/admin/Users
        [RequirePermission("User", "View")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<UserProfileDto>>>> GetUsers(
            [FromQuery] string? searchTerm,
            [FromQuery] int? roleId,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.Email != null && u.Email.ToLower().Contains(search) ||
                        u.FirstName != null && u.FirstName.ToLower().Contains(search) ||
                        u.LastName != null && u.LastName.ToLower().Contains(search));
                }

                // Lọc theo role
                if (roleId.HasValue)
                {
                    var userIdsInRole = await _context.UserRoles
                        .Where(ur => ur.RoleId == roleId.Value)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }

                // Lọc theo trạng thái
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserProfileDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserProfileDto
                    {
                        UserId = user.Id,
                        Email = user.Email ?? "",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DateOfBirth = user.DateOfBirth,
                        Gender = user.Gender,
                        RoleName = roles.FirstOrDefault() ?? "",
                        CreatedAt = user.CreatedAt
                    });
                }

                var result = new PagedResultDto<UserProfileDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(new ApiResponse<PagedResultDto<UserProfileDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách người dùng thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<UserProfileDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/admin/Users/{id}
        [RequirePermission("User", "View")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetUser(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var roleId = 0;
                if (roles.Any())
                {
                    var role = await _roleManager.FindByNameAsync(roles.First());
                    roleId = role?.Id ?? 0;
                }

                var userDetail = new UserDetailDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    RoleId = roleId,
                    RoleName = roles.FirstOrDefault() ?? "",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    TotalReviews = await _context.Reviews.CountAsync(r => r.UserId == id),
                    TotalFavorites = await _context.FavDocs.CountAsync(f => f.UserId == id),
                    TotalViews = await _context.ViewLogs.CountAsync(v => v.UserId == id),
                    TotalDownloads = await _context.DownloadLogs.CountAsync(d => d.UserId == id)
                };

                return Ok(new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Message = "Lấy thông tin người dùng thành công",
                    Data = userDetail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/admin/Users
        [RequirePermission("User", "Create")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> CreateUser(CreateUserDto createDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var existingUser = await _userManager.FindByEmailAsync(createDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    });
                }

                // Kiểm tra role tồn tại
                var role = await _roleManager.FindByIdAsync(createDto.RoleId.ToString());
                if (role == null)
                {
                    return BadRequest(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Role không tồn tại"
                    });
                }

                var user = new ApplicationUser
                {
                    UserName = createDto.Email,
                    Email = createDto.Email,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    DateOfBirth = createDto.DateOfBirth,
                    Gender = createDto.Gender,
                    IsActive = createDto.IsActive ?? true,
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, createDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, role.Name ?? "");

                var userDto = new UserProfileDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    RoleName = role.Name ?? "",
                    CreatedAt = user.CreatedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                    new ApiResponse<UserProfileDto>
                    {
                        Success = true,
                        Message = "Tạo người dùng thành công",
                        Data = userDto
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

        // PUT: api/admin/Users/{id}
        [RequirePermission("User", "Edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateUser(int id, UpdateUserDto updateDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Update role nếu thay đổi
                if (updateDto.RoleId.HasValue)
                {
                    var newRole = await _roleManager.FindByIdAsync(updateDto.RoleId.Value.ToString());
                    if (newRole == null)
                    {
                        return BadRequest(new ApiResponse<UserProfileDto>
                        {
                            Success = false,
                            Message = "Role không tồn tại"
                        });
                    }

                    // Remove old roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    // Add new role
                    await _userManager.AddToRoleAsync(user, newRole.Name ?? "");
                }

                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;

                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName;

                if (updateDto.DateOfBirth.HasValue)
                    user.DateOfBirth = updateDto.DateOfBirth;

                if (updateDto.Gender.HasValue)
                    user.Gender = updateDto.Gender;

                if (updateDto.IsActive.HasValue)
                    user.IsActive = updateDto.IsActive.Value;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserProfileDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    RoleName = roles.FirstOrDefault() ?? "",
                    CreatedAt = user.CreatedAt
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Cập nhật người dùng thành công",
                    Data = userDto
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

        // DELETE: api/admin/Users/{id}
        [RequirePermission("User", "Delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            try
            {
                // Không cho phép xóa chính mình
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == id)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không thể xóa tài khoản của chính mình"
                    });
                }

                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa người dùng thành công"
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

        // POST: api/admin/Users/{id}/reset-password
        [RequirePermission("User", "Edit")]
        [HttpPost("{id}/reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(int id, [FromBody] ResetPasswordDto resetDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Remove old password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, resetDto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Reset mật khẩu thành công"
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

        // POST: api/admin/Users/{id}/toggle-active
        [RequirePermission("User", "Edit")]
        [HttpPost("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleActive(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = user.IsActive ? "Kích hoạt tài khoản thành công" : "Khóa tài khoản thành công",
                    Data = new { IsActive = user.IsActive }
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

        // GET: api/admin/Users/statistics
        [RequirePermission("User", "View")]
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<AdminUserStatisticsDto>>> GetStatistics()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

                var usersByRole = new List<RoleStatDto>();
                var roles = await _roleManager.Roles.ToListAsync();

                foreach (var role in roles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                    usersByRole.Add(new RoleStatDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name ?? "",
                        UserCount = usersInRole.Count
                    });
                }

                var statistics = new AdminUserStatisticsDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    UsersByRole = usersByRole,
                    NewUsersThisMonth = await _context.Users
                        .CountAsync(u => u.CreatedAt >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                };

                return Ok(new ApiResponse<AdminUserStatisticsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AdminUserStatisticsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}