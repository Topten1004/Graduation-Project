using GradutionProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GradutionProject.Controllers;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using GradutionProject.ViewModel;

namespace GradutionProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext _context;

        int tokenExp = 3600 * 24 * 30;

        public MainVM mainData;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("Graduation-Access-Token");

            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult Update(MainVM mainVM)
        {
            // Assuming you have a method to update the database with the selected courses
            UpdateCourses(mainVM.CourseStatusVMList);

            // Redirect to another action or view after updating the database
            return RedirectToAction("Index");
        }

        private void UpdateCourses(List<CourseStatusVM> courseStatusList)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the records to delete based on the condition
            var recordsToDelete = _context.CourseStatusList.Where(x => x.UserId == userId);

            // Remove the records from the context
            _context.CourseStatusList.RemoveRange(recordsToDelete);

            // Save changes to the database
            _context.SaveChanges();

            // Iterate through the submitted course status list and update the database
            foreach (var courseStatus in courseStatusList)
            {
                if (courseStatus.IsPass)
                {
                    CourseStatus item = new CourseStatus();

                    item.UserId = userId;
                    item.CourseId = courseStatus.CourseId;

                    _context.CourseStatusList.Add(item);
                }
            }

            // Save changes to the database
            _context.SaveChanges();

        }

        // Action to display the main index page
        public IActionResult Index()
        {
            // Create a new instance of the MainVM
            MainVM mainData = new MainVM();

            // Get the user ID from the claims in the current user's identity
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the user information from the database
            var user = _context.Users.Where(x => x.Id == userId).FirstOrDefault();

            // List to store remaining courses for the user
            var remainCourse = new List<Course>();

            // Get the list of course status records from the database
            var courseStatusList = _context.CourseStatusList.ToList();

            // Iterate through all courses to find remaining courses for the user
            foreach (var item in _context.Courses)
            {
                if (item.MajorId == user.MajorId)
                {
                    // Check if the user has passed the course
                    bool flag = courseStatusList.Any(x => x.UserId == userId && x.CourseId == item.Id);

                    // If the user has not passed the course, add it to the list of remaining courses
                    if (flag == false)
                    {
                        remainCourse.Add(item);
                    }
                }
            }

            // Order the remaining courses by level and take the top 6
            mainData.SuggestCourses = remainCourse.OrderBy(x => x.level).Take(6).ToList();

            // Iterate through all courses again to get the course status information
            foreach (var item in _context.Courses)
            {
                if (item.MajorId == user.MajorId)
                {
                    // Create a CourseStatusVM instance to hold course status information
                    CourseStatusVM tempItem = new CourseStatusVM();

                    // Populate properties of the CourseStatusVM instance
                    tempItem.CourseId = item.Id;
                    tempItem.UserId = userId;
                    tempItem.CourseName = item.CourseName;
                    tempItem.Hours = item.Hours;
                    tempItem.CourseKey = item.CourseKey;

                    // Check if the user has passed the course
                    bool flag = courseStatusList.Any(x => x.UserId == userId && x.CourseId == item.Id);

                    // Set the IsPass property based on whether the user has passed the course
                    tempItem.IsPass = flag;

                    // Add the CourseStatusVM instance to the list in the mainData
                    mainData.CourseStatusVMList.Add(tempItem);
                }
            }

            // Create a new StatusVM instance to hold status information
            mainData.StatusData = new StatusVM();

            // Populate properties of the StatusVM instance
            mainData.StatusData.TotalCourse = mainData.CourseStatusVMList.Count();
            mainData.StatusData.PassedCourse = mainData.CourseStatusVMList.Where(x => x.IsPass == true).Count();
            mainData.StatusData.RemainCourse = mainData.CourseStatusVMList.Where(x => x.IsPass == false).Count();

            // Calculate the pass percentage and set it in the StatusVM instance
            mainData.StatusData.PassPercent = (double)mainData.StatusData.PassedCourse / mainData.StatusData.TotalCourse * 100;

            // Return the view with the mainData
            return View(mainData);
        }

        // Action to handle user login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model, string returnUrl)
        {
            // Create a new User instance
            User user = new User();

            // Set the returnUrl in ViewData
            ViewData["returnUrl"] = returnUrl;

            try
            {
                // Attempt to find the user in the database based on email and password
                user = _context.Users.Where(x => x.Email == model.Email && x.Password == model.Password).FirstOrDefault();

                // If the user is found
                if (user != null)
                {
                    // Create a list of claims for the user
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, "student")
                    };

                    // Get JWT configuration from appsettings.json
                    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));

                    // Set the expiration time for the token
                    DateTime expiration = DateTime.Now.AddSeconds(Convert.ToDouble(tokenExp));

                    // Create a JWT token
                    var token = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
                        issuer: config["JWT:ValidIssuer"],
                        audience: config["JWT:ValidAudience"],
                        subject: new ClaimsIdentity(authClaims),
                        notBefore: DateTime.Now,
                        expires: expiration,
                        issuedAt: DateTime.Now,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                        encryptingCredentials: new EncryptingCredentials(authSigningKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512));

                    // Add the JWT token to the response cookies
                    Response.Cookies.Append(
                        "Graduation-Access-Token",
                         new JwtSecurityTokenHandler().WriteToken(token),
                         new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Path = "/" }
                    );

                    // Redirect to the Index action in the Home controller
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // If the user is not found, set the returnUrl and redirect to the Error action
                    if (returnUrl == null) returnUrl = Request.Path;
                    return RedirectToAction("Error", "Home", new { msgErr = "Connection refused, incorrect password or email.", urlRetour = returnUrl });
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, set the returnUrl and redirect to the Error action
                if (returnUrl == null) returnUrl = Request.Path;
                return RedirectToAction("Error", "Home", new { msgErr = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            var viewModel = new SignUpVM
            {
                User = new User(),
                MajorList = _context.Majors.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpVM viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.User.UserRole = "Student";
                _context.Users.Add(viewModel.User);
                _context.SaveChanges();

                var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, viewModel.User.Id.ToString()),
                        new Claim(ClaimTypes.Name, viewModel.User.Email),
                        new Claim(ClaimTypes.Role, "student")
                    };

                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));

                DateTime expiration = DateTime.Now.AddSeconds(Convert.ToDouble(tokenExp));

                var token = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
                    issuer: config["JWT:ValidIssuer"],
                    audience: config["JWT:ValidAudience"],
                    subject: new ClaimsIdentity(authClaims),
                    notBefore: DateTime.Now,
                    expires: expiration,
                    issuedAt: DateTime.Now,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                    encryptingCredentials: new EncryptingCredentials(authSigningKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512));

                Response.Cookies.Append(
                    "Graduation-Access-Token",
                     new JwtSecurityTokenHandler().WriteToken(token),
                     new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict }
                );

                return RedirectToAction("Index", "Home");
            }

            // If the model is not valid, reload the majors and return to the signup page with validation errors
            viewModel.MajorList = _context.Majors.ToList();
            return View(viewModel);
        }

        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            
            return View();
        }

        public IActionResult Error(string msgErr, string urlReturn)
        {
            return View("Error", new string[] { msgErr, urlReturn });
        }
    }
}