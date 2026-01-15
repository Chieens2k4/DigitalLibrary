namespace DigitalLibrary.DTOs
{
    public class DocumentStatisticsDto
    {
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ViewLogDto> RecentViews { get; set; } = new();
        public List<DownloadLogDto> RecentDownloads { get; set; } = new();
    }
}
