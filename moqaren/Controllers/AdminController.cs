// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moqaren.Models;
using System.Diagnostics;

namespace moqaren.Controllers
{
    public class AdminController : Controller
    {
        private readonly MoqarenContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminController> _logger;

        public AdminController(MoqarenContext context, IWebHostEnvironment webHostEnvironment, ILogger<AdminController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Debug()
        {
            return View();
        }

        public async Task<IActionResult> ProductMGMT()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null && image.Length > 0)
                    {
                        var uniqueFileName = GetUniqueFileName(image.FileName);
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        product.ImageURL = $"/images/products/{uniqueFileName}";
                    }

                    product.CreatedAt = DateTime.Now;
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Product added successfully!";
                    return RedirectToAction(nameof(ProductMGMT));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding product");
                    ModelState.AddModelError("", "Error adding product. Please try again.");
                }
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("ProductMGMT", await _context.Products.Include(p => p.Category).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(product.ProductID);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    if (image != null && image.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageURL))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                existingProduct.ImageURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        var uniqueFileName = GetUniqueFileName(image.FileName);
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        existingProduct.ImageURL = $"/images/products/{uniqueFileName}";
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Brand = product.Brand;
                    existingProduct.Model = product.Model;
                    existingProduct.CategoryID = product.CategoryID;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(ProductMGMT));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product");
                    ModelState.AddModelError("", "Error updating product. Please try again.");
                }
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("ProductMGMT", await _context.Products.Include(p => p.Category).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            try
            {
                // Delete image file if it exists
                if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        product.ImageURL.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["Error"] = "Error deleting product. Please try again.";
            }

            return RedirectToAction(nameof(ProductMGMT));
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                    + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                    + Path.GetExtension(fileName);
        }
    }
}