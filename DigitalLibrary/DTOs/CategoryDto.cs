namespace DigitalLibrary.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
    }
}
