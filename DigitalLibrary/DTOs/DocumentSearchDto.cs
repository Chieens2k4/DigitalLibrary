namespace DigitalLibrary.DTOs
{
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
}
