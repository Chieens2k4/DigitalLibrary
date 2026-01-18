using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Authorization;

namespace DigitalLibrary.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DashboardController(DigitalLibraryContext context)
        {
            _context = context;
        }

        [RequirePermission("Dashboard", "View")]
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetOverview()
        {
            try
            {
                var now = DateTime.Now;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);

                var overview = new DashboardOverviewDto
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalDocuments = await _context.Documents.CountAsync(),
                    TotalCategories = await _context.Categories.CountAsync(),
                    TotalReviews = await _context.Reviews.CountAsync(),
                    TotalViews = await _context.ViewLogs.CountAsync(),
                    TotalDownloads = await _context.DownloadLogs.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                    NewUsersThisMonth = await _context.Users
                        .CountAsync(u => u.CreatedAt >= firstDayOfMonth),
                    NewDocumentsThisMonth = await _context.Documents
                        .CountAsync(d => d.DateUploaded >= firstDayOfMonth),
                    ViewsThisMonth = await _context.ViewLogs
                        .CountAsync(v => v.Time >= firstDayOfMonth),
                    DownloadsThisMonth = await _context.DownloadLogs
                        .CountAsync(d => d.Time >= firstDayOfMonth),
                    NewUsersLastMonth = await _context.Users
                        .CountAsync(u => u.CreatedAt >= firstDayOfLastMonth && u.CreatedAt < firstDayOfMonth),
                    NewDocumentsLastMonth = await _context.Documents
                        .CountAsync(d => d.DateUploaded >= firstDayOfLastMonth && d.DateUploaded < firstDayOfMonth),
                    AverageRating = await _context.Reviews.AnyAsync()
                        ? await _context.Reviews.AverageAsync(r => r.StarRate)
                        : 0
                };

                return Ok(new ApiResponse<DashboardOverviewDto>
                {
                    Success = true,
                    Message = "Lấy thống kê tổng quan thành công",
                    Data = overview
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DashboardOverviewDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Dashboard", "View")]
        [HttpGet("top-documents")]
        public async Task<ActionResult<ApiResponse<TopDocumentsDto>>> GetTopDocuments([FromQuery] int count = 10)
        {
            try
            {
                var mostViewed = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .OrderByDescending(d => d.ViewLogs.Count)
                    .Take(count)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        Title = d.Title,
                        CategoryId = d.CategoryId,
                        CategoryName = d.Category != null ? d.Category.CategoryName : null,
                        AuthorName = d.AuthorName,
                        ViewCount = d.ViewLogs.Count,
                        DownloadCount = d.DownloadLogs.Count,
                        AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0,
                        ReviewCount = d.Reviews.Count
                    })
                    .ToListAsync();

                var mostDownloaded = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .OrderByDescending(d => d.DownloadLogs.Count)
                    .Take(count)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        Title = d.Title,
                        CategoryId = d.CategoryId,
                        CategoryName = d.Category != null ? d.Category.CategoryName : null,
                        AuthorName = d.AuthorName,
                        ViewCount = d.ViewLogs.Count,
                        DownloadCount = d.DownloadLogs.Count,
                        AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0,
                        ReviewCount = d.Reviews.Count
                    })
                    .ToListAsync();

                var topRated = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .Where(d => d.Reviews.Any())
                    .OrderByDescending(d => d.Reviews.Average(r => r.StarRate))
                    .ThenByDescending(d => d.Reviews.Count)
                    .Take(count)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        Title = d.Title,
                        CategoryId = d.CategoryId,
                        CategoryName = d.Category != null ? d.Category.CategoryName : null,
                        AuthorName = d.AuthorName,
                        ViewCount = d.ViewLogs.Count,
                        DownloadCount = d.DownloadLogs.Count,
                        AverageRating = d.Reviews.Average(r => r.StarRate),
                        ReviewCount = d.Reviews.Count
                    })
                    .ToListAsync();

                var result = new TopDocumentsDto
                {
                    MostViewed = mostViewed,
                    MostDownloaded = mostDownloaded,
                    TopRated = topRated
                };

                return Ok(new ApiResponse<TopDocumentsDto>
                {
                    Success = true,
                    Message = "Lấy danh sách tài liệu nổi bật thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TopDocumentsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Dashboard", "View")]
        [HttpGet("recent-activities")]
        public async Task<ActionResult<ApiResponse<RecentActivitiesDto>>> GetRecentActivities([FromQuery] int count = 20)
        {
            try
            {
                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(count)
                    .Select(u => new UserActivityDto
                    {
                        UserId = u.Id,
                        Email = u.Email ?? "",
                        Name = $"{u.FirstName} {u.LastName}".Trim(),
                        RoleName = "", // Will be filled via UserManager if needed
                        ActivityTime = u.CreatedAt,
                        ActivityType = "Đăng ký"
                    })
                    .ToListAsync();

                var recentDocuments = await _context.Documents
                    .Include(d => d.Category)
                    .OrderByDescending(d => d.DateUploaded)
                    .Take(count)
                    .Select(d => new DocumentActivityDto
                    {
                        DocumentId = d.DocumentId,
                        Title = d.Title,
                        CategoryName = d.Category != null ? d.Category.CategoryName : "",
                        ActivityTime = d.DateUploaded,
                        ActivityType = "Tải lên"
                    })
                    .ToListAsync();

                var recentReviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Document)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(count)
                    .Select(r => new ReviewActivityDto
                    {
                        ReviewId = r.ReviewId,
                        UserName = $"{r.User!.FirstName} {r.User.LastName}".Trim(),
                        DocumentTitle = r.Document!.Title,
                        StarRate = r.StarRate,
                        ActivityTime = r.CreatedAt,
                        ActivityType = "Đánh giá"
                    })
                    .ToListAsync();

                var result = new RecentActivitiesDto
                {
                    RecentUsers = recentUsers,
                    RecentDocuments = recentDocuments,
                    RecentReviews = recentReviews
                };

                return Ok(new ApiResponse<RecentActivitiesDto>
                {
                    Success = true,
                    Message = "Lấy hoạt động gần đây thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<RecentActivitiesDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [RequirePermission("Dashboard", "View")]
        [HttpGet("charts-data")]
        public async Task<ActionResult<ApiResponse<ChartsDataDto>>> GetChartsData([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-days).Date;

                var viewsByDay = await _context.ViewLogs
                    .Where(v => v.Time >= startDate)
                    .GroupBy(v => v.Time.Date)
                    .Select(g => new DailyStatDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                var downloadsByDay = await _context.DownloadLogs
                    .Where(d => d.Time >= startDate)
                    .GroupBy(d => d.Time.Date)
                    .Select(g => new DailyStatDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                var newUsersByDay = await _context.Users
                    .Where(u => u.CreatedAt >= startDate)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new DailyStatDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                var documentsByCategory = await _context.Categories
                    .Select(c => new CategoryStatDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        DocumentCount = c.Documents.Count
                    })
                    .OrderByDescending(c => c.DocumentCount)
                    .ToListAsync();

                var usersByRole = new List<RoleStatDto>();
                // This will need UserManager to get role stats - simplified here

                var result = new ChartsDataDto
                {
                    ViewsByDay = viewsByDay,
                    DownloadsByDay = downloadsByDay,
                    NewUsersByDay = newUsersByDay,
                    DocumentsByCategory = documentsByCategory,
                    UsersByRole = usersByRole
                };

                return Ok(new ApiResponse<ChartsDataDto>
                {
                    Success = true,
                    Message = "Lấy dữ liệu biểu đồ thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ChartsDataDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}