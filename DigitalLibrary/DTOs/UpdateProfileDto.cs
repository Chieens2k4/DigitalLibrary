using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.DTOs
{
    public class UpdateProfileDto
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? Gender { get; set; }
    }
}
