using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs
{
    // Authentication DTOs
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }

    // User DTOs
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Gender { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileDto
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }
    }

    // Document DTOs
    public class DocumentDto
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Abstract { get; set; }
        public string? AuthorName { get; set; }
        public int? PublishYear { get; set; }
        public string? FilePath { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public DateTime DateUploaded { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class CreateDocumentDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? Abstract { get; set; }

        [StringLength(200)]
        public string? AuthorName { get; set; }

        [Range(1900, 2100)]
        public int? PublishYear { get; set; }

        [StringLength(20)]
        public string AccessLevel { get; set; } = "Public";
    }

    public class UpdateDocumentDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(1000)]
        public string? Abstract { get; set; }

        [StringLength(200)]
        public string? AuthorName { get; set; }

        [Range(1900, 2100)]
        public int? PublishYear { get; set; }

        [StringLength(20)]
        public string? AccessLevel { get; set; }
    }

    // Category DTOs
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
    }

    // Review DTOs
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int DocumentId { get; set; }
        public string? Comment { get; set; }
        public int StarRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required(ErrorMessage = "DocumentId là bắt buộc")]
        public int DocumentId { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "Đánh giá sao là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int StarRate { get; set; }
    }

    // Statistics DTOs
    public class UserStatisticsDto
    {
        public int TotalViewed { get; set; }
        public int TotalDownloaded { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalReviews { get; set; }
    }

    public class DocumentStatisticsDto
    {
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ViewLogDto> RecentViews { get; set; } = new();
        public List<DownloadLogDto> RecentDownloads { get; set; } = new();
    }

    public class ViewLogDto
    {
        public int ViewLogId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Time { get; set; }
    }

    public class DownloadLogDto
    {
        public int DownloadLogId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Time { get; set; }
    }

    // Search and Filter DTOs
    public class DocumentSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? AuthorName { get; set; }
        public int? PublishYear { get; set; }
        public string? SortBy { get; set; } // title, date, views, downloads, rating
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // Response DTOs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    // Role DTOs
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    // ==================== ADMIN DTOs ====================

    // User Management DTOs
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role là bắt buộc")]
        public int RoleId { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public bool? IsActive { get; set; } = true;
    }

    public class UpdateUserDto
    {
        public int? RoleId { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UserDetailDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Gender { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
    }

    // Category Management DTOs
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    // Document Management DTOs
    public class DocumentDetailDto
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Abstract { get; set; }
        public string? AuthorName { get; set; }
        public int? PublishYear { get; set; }
        public string? FilePath { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public DateTime DateUploaded { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public int FavoriteCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    // Review Management DTOs
    public class ReviewDetailDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public int StarRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public List<RatingDistributionDto> RatingDistribution { get; set; } = new();
        public int ReviewsThisMonth { get; set; }
    }

    public class RatingDistributionDto
    {
        public int StarRate { get; set; }
        public int Count { get; set; }
    }

    // Statistics DTOs
    public class AdminUserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public List<RoleStatDto> UsersByRole { get; set; } = new();
        public int NewUsersThisMonth { get; set; }
    }

    public class AdminDocumentStatisticsDto
    {
        public int TotalDocuments { get; set; }
        public int PublicDocuments { get; set; }
        public int PrivateDocuments { get; set; }
        public int RestrictedDocuments { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
        public int TotalReviews { get; set; }
        public List<CategoryStatDto> DocumentsByCategory { get; set; } = new();
        public int NewDocumentsThisMonth { get; set; }
    }

    public class RoleStatDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class CategoryStatDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
    }

    // Dashboard DTOs
    public class DashboardOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalCategories { get; set; }
        public int TotalReviews { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewDocumentsThisMonth { get; set; }
        public int ViewsThisMonth { get; set; }
        public int DownloadsThisMonth { get; set; }
        public int NewUsersLastMonth { get; set; }
        public int NewDocumentsLastMonth { get; set; }
        public double AverageRating { get; set; }
    }

    public class TopDocumentsDto
    {
        public List<DocumentDto> MostViewed { get; set; } = new();
        public List<DocumentDto> MostDownloaded { get; set; } = new();
        public List<DocumentDto> TopRated { get; set; } = new();
    }

    public class RecentActivitiesDto
    {
        public List<UserActivityDto> RecentUsers { get; set; } = new();
        public List<DocumentActivityDto> RecentDocuments { get; set; } = new();
        public List<ReviewActivityDto> RecentReviews { get; set; } = new();
    }

    public class UserActivityDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime ActivityTime { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    public class DocumentActivityDto
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime ActivityTime { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    public class ReviewActivityDto
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public int StarRate { get; set; }
        public DateTime ActivityTime { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    public class ChartsDataDto
    {
        public List<DailyStatDto> ViewsByDay { get; set; } = new();
        public List<DailyStatDto> DownloadsByDay { get; set; } = new();
        public List<DailyStatDto> NewUsersByDay { get; set; } = new();
        public List<CategoryStatDto> DocumentsByCategory { get; set; } = new();
        public List<RoleStatDto> UsersByRole { get; set; } = new();
    }

    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    // ==================== PERMISSION MANAGEMENT DTOs ====================

    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }

    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100)]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ClaimValue { get; set; } = string.Empty;

        public bool? IsGranted { get; set; } = true;
    }

    public class UpdatePermissionDto
    {
        [Required]
        public bool IsGranted { get; set; }
    }

    public class RolePermissionGroupDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class RolePermissionDetailDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
        public List<PermissionGroupDto> Permissions { get; set; } = new();
    }

    public class PermissionGroupDto
    {
        public string ClaimType { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class BulkUpdatePermissionsDto
    {
        public List<PermissionUpdateItem> Updates { get; set; } = new();
    }

    public class PermissionUpdateItem
    {
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }

    public class AvailablePermissionDto
    {
        public string ClaimType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] AvailableActions { get; set; } = Array.Empty<string>();
    }
}