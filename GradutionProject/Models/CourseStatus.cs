using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GradutionProject.Models
{
    [Table("CourseStatus")]
    public class CourseStatus
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("CourseId")]

        public int CourseId { get; set; }
    }
}
