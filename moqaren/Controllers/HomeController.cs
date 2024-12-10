using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moqaren.Models;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace moqaren.Controllers
{
    public class HomeController : Controller
    {
        private void SetSecureCookie(string name, string value, int? expirationDays = null)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            if (expirationDays.HasValue)
            {
                options.Expires = DateTime.Now.AddDays(expirationDays.Value);
            }

            Response.Cookies.Append(name, value, options);
        }

        private readonly ILogger<HomeController> _logger;
        private readonly MoqarenContext _context;

        public HomeController(ILogger<HomeController> logger, MoqarenContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var featuredProducts = await _context.Products
                    .Include(p => p.ProductPrices)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.ProductPrices.Count)
                    .Take(6)
                    .ToListAsync();

                var popularCategories = await _context.Categories
                    .Include(c => c.Products)
                    .OrderByDescending(c => c.Products.Count)
                    .Take(4)
                    .ToListAsync();

                ViewBag.FeaturedProducts = featuredProducts;
                ViewBag.PopularCategories = popularCategories;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading home page");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while loading the home page. Please try again later."
                });
            }
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
            // Clear existing cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == hashedPassword);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Update last login
                user.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();

                // Set Session
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                // Set Cookies
                string userInfo = $"{user.FirstName} {user.LastName}";
                SetSecureCookie("UserName", userInfo, model.RememberMe ? 30 : null);
                SetSecureCookie("LastLogin", DateTime.Now.ToString("o"));
                SetSecureCookie("UserEmail", user.Email);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    City = model.City,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters")]
        public string? City { get; set; }
    }
}