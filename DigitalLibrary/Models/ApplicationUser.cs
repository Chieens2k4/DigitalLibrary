using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; } // true: Nam, false: Nữ

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
        public virtual ICollection<ViewLog> ViewLogs { get; set; } = new List<ViewLog>();
        public virtual ICollection<FavDoc> FavDocs { get; set; } = new List<FavDoc>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
