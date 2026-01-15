using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]  
        public string CategoryName { get; set; }
        public virtual IEnumerable<Document>? Documents{ get; set; }

    }
}
