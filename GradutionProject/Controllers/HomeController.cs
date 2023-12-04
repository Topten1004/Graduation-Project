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

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete("ruiz-Worktime-Access-Token");

            return RedirectToAction("Login", "Home");
        }

        public IActionResult Index()
        {
            return View();
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
                        "ruiz-Worktime-Access-Token",
                         new JwtSecurityTokenHandler().WriteToken(token),
                         new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict }
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
                    "ruiz-Worktime-Access-Token",
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