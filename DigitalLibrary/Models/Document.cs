using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        [StringLength(1000, ErrorMessage = "Tóm tắt không được vượt quá 1000 ký tự")]
        public string? Abstract { get; set; }

        [StringLength(200, ErrorMessage = "Tên tác giả không được vượt quá 200 ký tự")]
        public string? AuthorName { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
        public int? PublishYear { get; set; }

        [StringLength(500)]
        public string? FilePath { get; set; }

        [StringLength(20)]
        public string AccessLevel { get; set; } = "Public"; // Public, Private, Restricted

        public DateTime DateUploaded { get; set; } = DateTime.Now;

        public virtual Category? Category { get; set; }
        public virtual ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
        public virtual ICollection<ViewLog> ViewLogs { get; set; } = new List<ViewLog>();
        public virtual ICollection<FavDoc> FavDocs { get; set; } = new List<FavDoc>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
