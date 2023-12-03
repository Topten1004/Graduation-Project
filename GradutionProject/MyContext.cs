using GradutionProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GradutionProject
{
    public class MyContext:DbContext
    {
       
        public MyContext(DbContextOptions<MyContext> Option) : base(Option)
        {

        }
        public MyContext() : base() { }

        public virtual DbSet<Course> Courses { get; set; }  
        public virtual DbSet<Major> Majors { get; set; }

        public virtual DbSet<User> Users { get; set; }
    }
}
