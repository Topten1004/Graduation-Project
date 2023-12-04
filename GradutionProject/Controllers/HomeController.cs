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

        public IActionResult Index()
        {
            MainVM mainData = new MainVM();

            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = _context.Users.Where(x => x.Id == userId).FirstOrDefault();

            var courseStatusList = _context.CourseStatusList.ToList();

            foreach (var item in _context.Courses)
            {
                if (item.MajorId == user.MajorId)
                {
                    CourseStatusVM tempItem = new CourseStatusVM();

                    tempItem.CourseId = item.Id;
                    tempItem.UserId = userId;
                    tempItem.CourseName = item.CourseName;

                    bool flag = courseStatusList.Any(x => x.UserId == userId && x.CourseId == item.Id);

                    tempItem.IsPass = flag;

                    mainData.CourseStatusVMList.Add(tempItem);
                }
            }

            mainData.StatusData = new StatusVM();

            mainData.StatusData.TotalCourse = mainData.CourseStatusVMList.Count();
            mainData.StatusData.PassedCourse = mainData.CourseStatusVMList.Where(x => x.IsPass == true).Count();
            mainData.StatusData.RemainCourse = mainData.CourseStatusVMList.Where(x => x.IsPass == false).Count();
            mainData.StatusData.PassPercent = (double)mainData.StatusData.PassedCourse / mainData.StatusData.TotalCourse * 100;

            return View(mainData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model, string returnUrl)
        {
            User user = new User();
            ViewData["returnUrl"] = returnUrl;
            try
            {
                user = _context.Users.Where(x => x.Email == model.Email && x.Password == model.Password).FirstOrDefault();

                if (user != null )
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
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
                         new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Path = "/" }
                    );
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (returnUrl == null) returnUrl = Request.Path;
                    return RedirectToAction("Error", "Home", new { msgErr = "Connection refused, incorrect password or email.", urlRetour = returnUrl });
                }
            }
            catch (Exception ex)
            {
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
            if (User.Identity.IsAuthenticated && returnUrl == null)
                return NotFound();

            ViewData["returnUrl"] = returnUrl;

            var login = Request.Query.Where(q => q.Key == "login").FirstOrDefault();

            if (login.Key != null)
                ViewData["login"] = login.Value;
            else
                ViewData["login"] = "";
            return View();
        }

        public IActionResult Error(string msgErr, string urlRetour)
        {
            return View("Error", new string[] { msgErr, urlRetour });
        }
    }
}