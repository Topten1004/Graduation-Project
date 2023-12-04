using System.ComponentModel.DataAnnotations;

namespace GradutionProject.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int MajorId { get; set; }

        public string UserRole { get; set; } = string.Empty;
    }
}
