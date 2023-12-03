namespace GradutionProject.Models
{
    public class ProgressViewModel
    {
        public M SelectedMajor { get; set; }
        public List<A> Courses { get; set; }
        public List<string> CompletedCourses { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
