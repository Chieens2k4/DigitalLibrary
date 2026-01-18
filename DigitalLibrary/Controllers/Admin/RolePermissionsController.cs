using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public RolePermissionsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/admin/RolePermissions
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RolePermissionGroupDto>>>> GetAllRolePermissions()
        {
            try
            {
                var roles = await _context.Roles
                    .Include(r => r.RoleClaims)
                    .OrderBy(r => r.Id)
                    .ToListAsync();

                var result = roles.Select(r => new RolePermissionGroupDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? "",
                    Description = r.Description,
                    Permissions = r.RoleClaims.Select(rc => new PermissionDto
                    {
                        PermissionId = rc.RoleClaimId,
                        ClaimType = rc.ClaimType,
                        ClaimValue = rc.ClaimValue,
                        IsGranted = rc.IsGranted
                    }).ToList()
                }).ToList();

                return Ok(new ApiResponse<List<RolePermissionGroupDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách quyền thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<RolePermissionGroupDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/admin/RolePermissions/{roleId}
        [HttpGet("{roleId}")]
        public async Task<ActionResult<ApiResponse<RolePermissionDetailDto>>> GetRolePermissions(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RoleClaims)
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null)
                {
                    return NotFound(new ApiResponse<RolePermissionDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy role"
                    });
                }

                var result = new RolePermissionDetailDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? "",
                    Description = role.Description,
                    CreatedAt = role.CreatedAt,
                    UserCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == roleId),
                    Permissions = role.RoleClaims
                        .GroupBy(rc => rc.ClaimType)
                        .Select(g => new PermissionGroupDto
                        {
                            ClaimType = g.Key,
                            Permissions = g.Select(rc => new PermissionDto
                            {
                                PermissionId = rc.RoleClaimId,
                                ClaimType = rc.ClaimType,
                                ClaimValue = rc.ClaimValue,
                                IsGranted = rc.IsGranted
                            }).ToList()
                        }).ToList()
                };

                return Ok(new ApiResponse<RolePermissionDetailDto>
                {
                    Success = true,
                    Message = "Lấy thông tin quyền thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RolePermissionDetailDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/admin/RolePermissions/{roleId}/permission/{permissionId}
        [HttpPut("{roleId}/permission/{permissionId}")]
        public async Task<ActionResult<ApiResponse<PermissionDto>>> UpdatePermission(
            int roleId,
            int permissionId,
            [FromBody] UpdatePermissionDto updateDto)
        {
            try
            {
                var permission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleClaimId == permissionId && rp.RoleId == roleId);

                if (permission == null)
                {
                    return NotFound(new ApiResponse<PermissionDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy quyền"
                    });
                }

                permission.IsGranted = updateDto.IsGranted;
                await _context.SaveChangesAsync();

                var result = new PermissionDto
                {
                    PermissionId = permission.RoleClaimId,
                    ClaimType = permission.ClaimType,
                    ClaimValue = permission.ClaimValue,
                    IsGranted = permission.IsGranted
                };

                return Ok(new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Message = updateDto.IsGranted ? "Đã cấp quyền" : "Đã thu hồi quyền",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/admin/RolePermissions/{roleId}/permission
        [HttpPost("{roleId}/permission")]
        public async Task<ActionResult<ApiResponse<PermissionDto>>> AddPermission(
            int roleId,
            [FromBody] CreatePermissionDto createDto)
        {
            try
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    return NotFound(new ApiResponse<PermissionDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy role"
                    });
                }

                // Kiểm tra permission đã tồn tại chưa
                var exists = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId &&
                                   rp.ClaimType == createDto.ClaimType &&
                                   rp.ClaimValue == createDto.ClaimValue);

                if (exists)
                {
                    return BadRequest(new ApiResponse<PermissionDto>
                    {
                        Success = false,
                        Message = "Quyền này đã tồn tại"
                    });
                }

                var permission = new RoleClaim
                {
                    RoleId = roleId,
                    ClaimType = createDto.ClaimType,
                    ClaimValue = createDto.ClaimValue,
                    IsGranted = createDto.IsGranted ?? true,
                    CreatedAt = DateTime.Now
                };

                _context.RolePermissions.Add(permission);
                await _context.SaveChangesAsync();

                var result = new PermissionDto
                {
                    PermissionId = permission.RoleClaimId,
                    ClaimType = permission.ClaimType,
                    ClaimValue = permission.ClaimValue,
                    IsGranted = permission.IsGranted
                };

                return CreatedAtAction(nameof(GetRolePermissions), new { roleId },
                    new ApiResponse<PermissionDto>
                    {
                        Success = true,
                        Message = "Thêm quyền thành công",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // DELETE: api/admin/RolePermissions/{roleId}/permission/{permissionId}
        [HttpDelete("{roleId}/permission/{permissionId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeletePermission(int roleId, int permissionId)
        {
            try
            {
                var permission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleClaimId == permissionId && rp.RoleId == roleId);

                if (permission == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy quyền"
                    });
                }

                _context.RolePermissions.Remove(permission);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa quyền thành công"
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

        // PUT: api/admin/RolePermissions/{roleId}/bulk-update
        [HttpPut("{roleId}/bulk-update")]
        public async Task<ActionResult<ApiResponse<object>>> BulkUpdatePermissions(
            int roleId,
            [FromBody] BulkUpdatePermissionsDto bulkUpdateDto)
        {
            try
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy role"
                    });
                }

                foreach (var update in bulkUpdateDto.Updates)
                {
                    var permission = await _context.RolePermissions
                        .FirstOrDefaultAsync(rp =>
                            rp.RoleId == roleId &&
                            rp.ClaimType == update.ClaimType &&
                            rp.ClaimValue == update.ClaimValue);

                    if (permission != null)
                    {
                        permission.IsGranted = update.IsGranted;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Cập nhật {bulkUpdateDto.Updates.Count} quyền thành công"
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

        // GET: api/admin/RolePermissions/available-permissions
        [HttpGet("available-permissions")]
        public ActionResult<ApiResponse<List<AvailablePermissionDto>>> GetAvailablePermissions()
        {
            var availablePermissions = new List<AvailablePermissionDto>
            {
                new AvailablePermissionDto
                {
                    ClaimType = "User",
                    Description = "Quản lý người dùng",
                    AvailableActions = new[] { "View", "Create", "Edit", "Delete" }
                },
                new AvailablePermissionDto
                {
                    ClaimType = "Document",
                    Description = "Quản lý tài liệu",
                    AvailableActions = new[] { "View", "Create", "Edit", "Delete", "Download", "Upload" }
                },
                new AvailablePermissionDto
                {
                    ClaimType = "Category",
                    Description = "Quản lý danh mục",
                    AvailableActions = new[] { "View", "Create", "Edit", "Delete" }
                },
                new AvailablePermissionDto
                {
                    ClaimType = "Review",
                    Description = "Quản lý đánh giá",
                    AvailableActions = new[] { "View", "Create", "Edit", "Delete", "Moderate" }
                },
                new AvailablePermissionDto
                {
                    ClaimType = "Dashboard",
                    Description = "Dashboard",
                    AvailableActions = new[] { "View", "Export" }
                },
                new AvailablePermissionDto
                {
                    ClaimType = "System",
                    Description = "Cấu hình hệ thống",
                    AvailableActions = new[] { "Configure", "Backup" }
                }
            };

            return Ok(new ApiResponse<List<AvailablePermissionDto>>
            {
                Success = true,
                Message = "Lấy danh sách quyền khả dụng thành công",
                Data = availablePermissions
            });
        }
    }
}