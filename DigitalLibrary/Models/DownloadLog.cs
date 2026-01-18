using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class DownloadLog
    {
        [Key]
        public int DownloadLogId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Document))]
        public int DocumentId { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public virtual ApplicationUser? User { get; set; }
        public virtual Document? Document { get; set; }
    }
}
