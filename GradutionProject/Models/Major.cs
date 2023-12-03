using System.ComponentModel.DataAnnotations;

namespace GradutionProject.Models
{
    public class Major
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
