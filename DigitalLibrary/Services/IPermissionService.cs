using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
using System.Security.Claims;

namespace DigitalLibrary.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string claimType, string claimValue);
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string claimType, string claimValue);
        Task<List<RoleClaim>> GetUserPermissionsAsync(int userId);
        Task<List<RoleClaim>> GetRolePermissionsAsync(int roleId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly DigitalLibraryContext _context;

        public PermissionService(DigitalLibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int userId, string claimType, string claimValue)
        {
            // Lấy roles của user
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return false;

            // Kiểm tra có permission trong bất kỳ role nào không
            var hasPermission = await _context.RolePermissions
                .AnyAsync(rp =>
                    userRoles.Contains(rp.RoleId) &&
                    rp.ClaimType == claimType &&
                    rp.ClaimValue == claimValue &&
                    rp.IsGranted);

            return hasPermission;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string claimType, string claimValue)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return false;

            return await HasPermissionAsync(userId, claimType, claimValue);
        }

        public async Task<List<RoleClaim>> GetUserPermissionsAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return new List<RoleClaim>();

            var permissions = await _context.RolePermissions
                .Where(rp => userRoles.Contains(rp.RoleId) && rp.IsGranted)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<List<RoleClaim>> GetRolePermissionsAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }
    }
}