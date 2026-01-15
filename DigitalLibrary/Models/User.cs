using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalLibrary.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; } // true: Nam, false: Nữ

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public virtual Role? Role { get; set; }
        public virtual ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();
        public virtual ICollection<ViewLog> ViewLogs { get; set; } = new List<ViewLog>();
        public virtual ICollection<FavDoc> FavDocs { get; set; } = new List<FavDoc>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
