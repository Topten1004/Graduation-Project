using Humanizer.Localisation.TimeToClockNotation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradutionProject.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CourseKey { get; set; }
        public int MajorId { get; set; }
        public int Hours { get; set; }
        public string CourseName { get; set; }
        public int CourseNumber {  get; set; }
        public int level { get; set; }

    }
}
