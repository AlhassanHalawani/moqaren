// Create a new file Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;

namespace moqaren.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Debug()
        {
            return View();
        }
    }
}