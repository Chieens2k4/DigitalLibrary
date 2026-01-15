using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs
{
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

}
