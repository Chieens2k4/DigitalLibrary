using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public virtual User? User{ get; set; }
        [ForeignKey(nameof(Document))]  
        public int DocumentId { get; set; }
        public virtual Document? Document{ get; set; }
        [MaxLength(100)]
        public string Comment{ get; set; }
        [Range(1,5)]
        public int StarRate{ get; set; }
    }
}
