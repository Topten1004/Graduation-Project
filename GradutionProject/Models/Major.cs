using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradutionProject.Models
{
    [Table("Majors")]

    public class Major
    {
        public Major() {
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
