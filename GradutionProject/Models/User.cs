using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradutionProject.Models
{
    [Table("Users")] 
    // Move the Table attribute here
    public class User
    {
        public User()
        {
        }

        [Key]
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string AcademicNumber { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int MajorId { get; set; }

        public string UserRole { get; set; } = string.Empty;
    }
}
