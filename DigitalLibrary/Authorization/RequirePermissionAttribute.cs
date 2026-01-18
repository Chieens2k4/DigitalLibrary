using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DigitalLibrary.Services;
using System.Security.Claims;

namespace DigitalLibrary.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string ClaimType { get; }
        public string ClaimValue { get; }

        public RequirePermissionAttribute(string claimType, string claimValue)
        {
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra user đã authenticate chưa
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Message = "Vui lòng đăng nhập"
                });
                return;
            }

            // Lấy PermissionService từ DI
            var permissionService = context.HttpContext.RequestServices
                .GetService<IPermissionService>();

            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Kiểm tra permission
            var hasPermission = await permissionService.HasPermissionAsync(
                context.HttpContext.User,
                ClaimType,
                ClaimValue
            );

            if (!hasPermission)
            {
                context.Result = new ObjectResult(new
                {
                    Success = false,
                    Message = "Bạn không có quyền thực hiện hành động này"
                })
                {
                    StatusCode = 403
                };
            }
        }
    }
}