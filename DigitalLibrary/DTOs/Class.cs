namespace DigitalLibrary.DTOs
{
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
}
