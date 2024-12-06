using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moqaren.Models;
using System.Diagnostics;

namespace moqaren.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MoqarenContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(MoqarenContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductPrices)
                        .ThenInclude(pp => pp.Retailer)
                    .ToListAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while retrieving products. Please try again later."
                });
            }
        }

        public async Task<IActionResult> Compare(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductPrices)
                        .ThenInclude(pp => pp.Retailer)
                    .Include(p => p.PriceHistory)
                    .FirstOrDefaultAsync(m => m.ProductID == id);

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving product details");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while retrieving product details. Please try again later."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetPriceAlert(int productId, decimal targetPrice)
        {
            try
            {
                // TODO: Get actual user ID after implementing authentication
                const int temporaryUserId = 1;

                var alert = new PriceAlert
                {
                    UserID = temporaryUserId,
                    ProductID = productId,
                    TargetPrice = targetPrice,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.PriceAlerts.Add(alert);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting price alert");
                return Json(new { success = false, message = "Error setting price alert" });
            }
        }
    }
}