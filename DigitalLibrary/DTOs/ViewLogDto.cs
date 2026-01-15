namespace DigitalLibrary.DTOs
{
    public class ViewLogDto
    {
        public int ViewLogId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Time { get; set; }
    }
}
