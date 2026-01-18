using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Models
{
    public class RoleClaim
    {
        [Key]
        public int RoleClaimId { get; set; }

        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClaimType { get; set; } = string.Empty; // VD: "Document", "User", "Category"

        [Required]
        [StringLength(100)]
        public string ClaimValue { get; set; } = string.Empty; // VD: "View", "Create", "Edit", "Delete", "Download"

        public bool IsGranted { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ApplicationRole? Role { get; set; }
    }
}
