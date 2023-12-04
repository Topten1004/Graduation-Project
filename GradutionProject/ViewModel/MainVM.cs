using GradutionProject.Models;

namespace GradutionProject.ViewModel
{
    public class MainVM
    {
        public MainVM() {

            CourseStatusVMList = new List<CourseStatusVM>();
        }

        public StatusVM StatusData { get; set; }
        public List<CourseStatusVM> CourseStatusVMList { get; set; }

    }
}
