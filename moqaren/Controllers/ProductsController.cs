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

        public async Task<IActionResult> Index(int? categoryId, string searchTerm)
        {
            try
            {
                var productsQuery = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductPrices)
                    .AsQueryable();

                if (categoryId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryID == categoryId);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    productsQuery = productsQuery.Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm));
                }

                ViewData["Categories"] = await _context.Categories.ToListAsync();
                var products = await productsQuery.ToListAsync();
                return View("productIndex", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id });
            }
        }

        public async Task<IActionResult> Compare(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductPrices)
                    .ThenInclude(pp => pp.Retailer)
                .Include(p => p.PriceHistory)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}