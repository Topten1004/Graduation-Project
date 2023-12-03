using GradutionProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GradutionProject.Controllers;
using System.Diagnostics;

namespace GradutionProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext _context;

     
        private readonly List<Major> _countries;
        private readonly List<Course> _cities;

        public HomeController(MyContext context)
        {
            _context = context;

            // Initialize dummy data for countries and cities
            _countries = new List<Major>
        {
       
            new Major { Id = 1, Name = "Computer Science" },
            new Major { Id = 2, Name = "Software Engineering" },
            new Major { Id = 3, Name = "Computer Engineering" }
        };

            _cities = _context.Courses.ToList();
        }
       
        public IActionResult Index()
        {
            var viewModel = new CascadingDropdownViewModel
            {
                Countries = _context.Majors.ToList(),
                Cities = new List<Course>()
            };

            return View(viewModel);
        }

        public IActionResult LoadCities(int countryId)
        {
            var cities = _cities.Where(c => c.CourseID   == countryId).ToList();
            return Json(cities);
        }
    }
}