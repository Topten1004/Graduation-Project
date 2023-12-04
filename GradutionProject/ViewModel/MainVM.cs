using GradutionProject.Models;

namespace GradutionProject.ViewModel
{
    public class MainVM
    {
        public MainVM() {

            SuggestCourses = new List<Course>();
            CourseStatusVMList = new List<CourseStatusVM>();
        }

        public StatusVM StatusData { get; set; }
        public List<CourseStatusVM> CourseStatusVMList { get; set; }

        public List<Course> SuggestCourses { get; set; }
    }
}
