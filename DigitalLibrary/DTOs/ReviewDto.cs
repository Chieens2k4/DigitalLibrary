namespace DigitalLibrary.DTOs
{
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
}
