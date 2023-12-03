namespace GradutionProject.Models
{
    public class CascadingDropdownViewModel
    {
        public int SelectedCountryId { get; set; }
        public int SelectedCityId { get; set; }
        public List<Major> Countries { get; set; }
        public List<Course> Cities { get; set; }
    }
}
