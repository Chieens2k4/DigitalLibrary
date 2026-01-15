using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Document))]
        public int DocumentId { get; set; }

        [StringLength(500, ErrorMessage = "Bình luận không được vượt quá 500 ký tự")]
        public string? Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [Required(ErrorMessage = "Đánh giá sao là bắt buộc")]
        public int StarRate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual User? User { get; set; }
        public virtual Document? Document { get; set; }
    }
}
