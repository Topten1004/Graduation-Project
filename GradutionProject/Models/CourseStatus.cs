using System.ComponentModel.DataAnnotations;

namespace GradutionProject.Models
{
    public class CourseStatus
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CourseId { get; set; }
    }
}
