using Humanizer.Localisation.TimeToClockNotation;
using System.ComponentModel.DataAnnotations;

namespace GradutionProject.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CourseKey { get; set; }
        public int MajorID { get; set; }
        public int Hours { get; set; }
        public string CourseName { get; set; }
        public int CourseNumber {  get; set; }
        public int level { get; set; }
    }
}
