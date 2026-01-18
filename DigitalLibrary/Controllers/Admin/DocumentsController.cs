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
    public class DocumentsController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;

        public DocumentsController(DigitalLibraryContext context)
        {
            _context = context;
        }

        // GET: api/admin/Documents
        [RequirePermission("Document", "View")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<DocumentDto>>>> GetDocuments(
            [FromQuery] string? searchTerm,
            [FromQuery] int? categoryId,
            [FromQuery] string? accessLevel,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(d =>
                        d.Title.ToLower().Contains(search) ||
                        (d.Abstract != null && d.Abstract.ToLower().Contains(search)) ||
                        (d.AuthorName != null && d.AuthorName.ToLower().Contains(search)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(d => d.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(accessLevel))
                {
                    query = query.Where(d => d.AccessLevel == accessLevel);
                }

                var totalCount = await query.CountAsync();

                var documents = await query
                    .OrderByDescending(d => d.DateUploaded)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var documentDtos = documents.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category?.CategoryName,
                    Abstract = d.Abstract,
                    AuthorName = d.AuthorName,
                    PublishYear = d.PublishYear,
                    FilePath = d.FilePath,
                    AccessLevel = d.AccessLevel,
                    DateUploaded = d.DateUploaded,
                    ViewCount = d.ViewLogs.Count,
                    DownloadCount = d.DownloadLogs.Count,
                    AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = d.Reviews.Count
                }).ToList();

                var result = new PagedResultDto<DocumentDto>
                {
                    Items = documentDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(new ApiResponse<PagedResultDto<DocumentDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách tài liệu thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<DocumentDto>>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/admin/Documents/{id}
        [RequirePermission("Document", "View")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> GetDocument(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .Include(d => d.FavDocs)
                    .FirstOrDefaultAsync(d => d.DocumentId == id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<DocumentDetailDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var documentDetail = new DocumentDetailDto
                {
                    DocumentId = document.DocumentId,
                    Title = document.Title,
                    CategoryId = document.CategoryId,
                    CategoryName = document.Category?.CategoryName,
                    Abstract = document.Abstract,
                    AuthorName = document.AuthorName,
                    PublishYear = document.PublishYear,
                    FilePath = document.FilePath,
                    AccessLevel = document.AccessLevel,
                    DateUploaded = document.DateUploaded,
                    ViewCount = document.ViewLogs.Count,
                    DownloadCount = document.DownloadLogs.Count,
                    FavoriteCount = document.FavDocs.Count,
                    AverageRating = document.Reviews.Any() ? document.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = document.Reviews.Count
                };

                return Ok(new ApiResponse<DocumentDetailDto>
                {
                    Success = true,
                    Message = "Lấy thông tin tài liệu thành công",
                    Data = documentDetail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentDetailDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/admin/Documents
        [RequirePermission("Document", "Create")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<DocumentDto>>> CreateDocument(CreateDocumentDto createDto)
        {
            try
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == createDto.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new ApiResponse<DocumentDto>
                    {
                        Success = false,
                        Message = "Danh mục không tồn tại"
                    });
                }

                var document = new Document
                {
                    Title = createDto.Title,
                    CategoryId = createDto.CategoryId,
                    Abstract = createDto.Abstract,
                    AuthorName = createDto.AuthorName,
                    PublishYear = createDto.PublishYear,
                    AccessLevel = createDto.AccessLevel,
                    DateUploaded = DateTime.Now
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                await _context.Entry(document).Reference(d => d.Category).LoadAsync();

                var documentDto = new DocumentDto
                {
                    DocumentId = document.DocumentId,
                    Title = document.Title,
                    CategoryId = document.CategoryId,
                    CategoryName = document.Category?.CategoryName,
                    Abstract = document.Abstract,
                    AuthorName = document.AuthorName,
                    PublishYear = document.PublishYear,
                    FilePath = document.FilePath,
                    AccessLevel = document.AccessLevel,
                    DateUploaded = document.DateUploaded,
                    ViewCount = 0,
                    DownloadCount = 0,
                    AverageRating = 0,
                    ReviewCount = 0
                };

                return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId },
                    new ApiResponse<DocumentDto>
                    {
                        Success = true,
                        Message = "Tạo tài liệu thành công",
                        Data = documentDto
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/admin/Documents/{id}
        [RequirePermission("Document", "Edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDto>>> UpdateDocument(int id, UpdateDocumentDto updateDto)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Category)
                    .Include(d => d.ViewLogs)
                    .Include(d => d.DownloadLogs)
                    .Include(d => d.Reviews)
                    .FirstOrDefaultAsync(d => d.DocumentId == id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<DocumentDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                if (updateDto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == updateDto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        return BadRequest(new ApiResponse<DocumentDto>
                        {
                            Success = false,
                            Message = "Danh mục không tồn tại"
                        });
                    }
                    document.CategoryId = updateDto.CategoryId.Value;
                }

                if (!string.IsNullOrEmpty(updateDto.Title))
                    document.Title = updateDto.Title;

                if (updateDto.Abstract != null)
                    document.Abstract = updateDto.Abstract;

                if (updateDto.AuthorName != null)
                    document.AuthorName = updateDto.AuthorName;

                if (updateDto.PublishYear.HasValue)
                    document.PublishYear = updateDto.PublishYear;

                if (!string.IsNullOrEmpty(updateDto.AccessLevel))
                    document.AccessLevel = updateDto.AccessLevel;

                await _context.SaveChangesAsync();

                await _context.Entry(document).Reference(d => d.Category).LoadAsync();

                var documentDto = new DocumentDto
                {
                    DocumentId = document.DocumentId,
                    Title = document.Title,
                    CategoryId = document.CategoryId,
                    CategoryName = document.Category?.CategoryName,
                    Abstract = document.Abstract,
                    AuthorName = document.AuthorName,
                    PublishYear = document.PublishYear,
                    FilePath = document.FilePath,
                    AccessLevel = document.AccessLevel,
                    DateUploaded = document.DateUploaded,
                    ViewCount = document.ViewLogs.Count,
                    DownloadCount = document.DownloadLogs.Count,
                    AverageRating = document.Reviews.Any() ? document.Reviews.Average(r => r.StarRate) : 0,
                    ReviewCount = document.Reviews.Count
                };

                return Ok(new ApiResponse<DocumentDto>
                {
                    Success = true,
                    Message = "Cập nhật tài liệu thành công",
                    Data = documentDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // DELETE: api/admin/Documents/{id}
        [RequirePermission("Document", "Delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa tài liệu thành công"
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

        // POST: api/admin/Documents/{id}/upload-file
        [RequirePermission("Document", "Upload")]
        [HttpPost("{id}/upload-file")]
        public async Task<ActionResult<ApiResponse<object>>> UploadFile(int id, [FromForm] IFormFile file)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "File không hợp lệ"
                    });
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                document.FilePath = $"/uploads/{fileName}";
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Upload file thành công",
                    Data = new { FilePath = document.FilePath }
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

        // GET: api/admin/Documents/statistics
        [RequirePermission("Document", "View")]
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<AdminDocumentStatisticsDto>>> GetStatistics()
        {
            try
            {
                var statistics = new AdminDocumentStatisticsDto
                {
                    TotalDocuments = await _context.Documents.CountAsync(),
                    PublicDocuments = await _context.Documents.CountAsync(d => d.AccessLevel == "Public"),
                    PrivateDocuments = await _context.Documents.CountAsync(d => d.AccessLevel == "Private"),
                    RestrictedDocuments = await _context.Documents.CountAsync(d => d.AccessLevel == "Restricted"),
                    TotalViews = await _context.ViewLogs.CountAsync(),
                    TotalDownloads = await _context.DownloadLogs.CountAsync(),
                    TotalReviews = await _context.Reviews.CountAsync(),
                    DocumentsByCategory = await _context.Categories
                        .Select(c => new CategoryStatDto
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            DocumentCount = c.Documents.Count
                        })
                        .ToListAsync(),
                    NewDocumentsThisMonth = await _context.Documents
                        .CountAsync(d => d.DateUploaded >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                };

                return Ok(new ApiResponse<AdminDocumentStatisticsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AdminDocumentStatisticsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/admin/Documents/{id}/statistics
        [RequirePermission("Document", "View")]
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<ApiResponse<DocumentStatisticsDto>>> GetDocumentStatistics(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.ViewLogs).ThenInclude(v => v.User)
                    .Include(d => d.DownloadLogs).ThenInclude(dl => dl.User)
                    .Include(d => d.Reviews)
                    .FirstOrDefaultAsync(d => d.DocumentId == id);

                if (document == null)
                {
                    return NotFound(new ApiResponse<DocumentStatisticsDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }

                var statistics = new DocumentStatisticsDto
                {
                    TotalViews = document.ViewLogs.Count,
                    TotalDownloads = document.DownloadLogs.Count,
                    AverageRating = document.Reviews.Any() ? document.Reviews.Average(r => r.StarRate) : 0,
                    TotalReviews = document.Reviews.Count,
                    RecentViews = document.ViewLogs
                        .OrderByDescending(v => v.Time)
                        .Take(10)
                        .Select(v => new ViewLogDto
                        {
                            ViewLogId = v.ViewLogId,
                            UserId = v.UserId,
                            UserName = $"{v.User?.FirstName} {v.User?.LastName}".Trim(),
                            Time = v.Time
                        })
                        .ToList(),
                    RecentDownloads = document.DownloadLogs
                        .OrderByDescending(d => d.Time)
                        .Take(10)
                        .Select(d => new DownloadLogDto
                        {
                            DownloadLogId = d.DownloadLogId,
                            UserId = d.UserId,
                            UserName = $"{d.User?.FirstName} {d.User?.LastName}".Trim(),
                            Time = d.Time
                        })
                        .ToList()
                };

                return Ok(new ApiResponse<DocumentStatisticsDto>
                {
                    Success = true,
                    Message = "Lấy thống kê tài liệu thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentStatisticsDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}