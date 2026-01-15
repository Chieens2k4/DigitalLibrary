using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs
{
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
}
