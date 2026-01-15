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
        public virtual User? User{ get; set; }
        [ForeignKey(nameof(Document))]  
        public int DocumentId { get; set; }
        public virtual Document? Document{ get; set; }
        public DateTime Time{ get; set; }
    }
}
