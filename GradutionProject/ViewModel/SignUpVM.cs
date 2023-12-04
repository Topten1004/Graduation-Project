using GradutionProject.Models;

namespace GradutionProject.ViewModel
{
    public class SignUpVM
    {
        public SignUpVM() {
            
            User = new User();

            MajorList = new List<Major>();
        }

        public User User { get; set; }
        public List<Major> MajorList { get; set; }
    }
}
