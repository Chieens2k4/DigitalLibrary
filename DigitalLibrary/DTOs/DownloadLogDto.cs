namespace DigitalLibrary.DTOs
{
    public class DownloadLogDto
    {
        public int DownloadLogId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Time { get; set; }
    }
}
