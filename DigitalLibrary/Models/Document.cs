using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }
        [Required]
        public string Title{ get; set; }
        [ForeignKey(nameof(Category))]  
        public int CategoryId{ get; set; }
  
        public string Abstract{ get; set; }
        public string AuthorName{ get; set; }
        public int PublishYear{ get; set; }
        public string FilePath{ get; set; }
        public string AccessLevel{ get; set; }
        public DateTime DateUploaded{ get; set; }

        public virtual Category? Category { get; set; }
        public virtual IEnumerable<DownloadLog>?DownloadLogs{ get; set; }
        public virtual IEnumerable<ViewLog>?ViewLogs{ get; set; }
        public virtual IEnumerable<FavDoc>?FavDocs{ get; set; }
        public virtual IEnumerable<Review>?Reviews{ get; set; }

    }
}
