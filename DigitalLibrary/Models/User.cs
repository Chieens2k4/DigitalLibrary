using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [EmailAddress]
        public string Email{ get; set; }
        public string PasswordHash{ get; set; }
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
 
        public string? FirstName{ get; set; }
        public string? LastName{ get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Gender{ get; set; }

        public virtual Role? Role { get; set; }
        public virtual IEnumerable<DownloadLog>? DownloadLogs{ get; set; }
        public virtual IEnumerable<ViewLog>? ViewLogs{ get; set; }
        public virtual IEnumerable<FavDoc>? FavDocs{ get; set; }
        public virtual IEnumerable<Review>? Reviews{ get; set; }
    }
}
