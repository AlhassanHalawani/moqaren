using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moqaren.Models;
using System.Diagnostics;

namespace moqaren.Controllers
{
    // Add authorization later when implementing admin authentication
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MoqarenContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminController> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int MaxImageSize = 5 * 1024 * 1024; // 5MB

        public AdminController(
            MoqarenContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AdminController> logger)
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
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                ViewBag.Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product management page");
                TempData["Error"] = "Error loading products. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PrepareViewBagForError();
                    return View("ProductMGMT", await GetProductsForView());
                }

                if (image != null)
                {
                    var imageValidation = ValidateImage(image);
                    if (!imageValidation.IsValid)
                    {
                        ModelState.AddModelError("Image", imageValidation.ErrorMessage);
                        PrepareViewBagForError();
                        return View("ProductMGMT", await GetProductsForView());
                    }

                    var imageResult = await SaveImage(image);
                    if (!imageResult.Success)
                    {
                        ModelState.AddModelError("Image", imageResult.ErrorMessage);
                        PrepareViewBagForError();
                        return View("ProductMGMT", await GetProductsForView());
                    }

                    product.ImageURL = imageResult.ImagePath;
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
                TempData["Error"] = "Error adding product. Please try again.";
                return RedirectToAction(nameof(ProductMGMT));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct([FromForm] Product product, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PrepareViewBagForError();
                    return View("ProductMGMT", await GetProductsForView());
                }

                var existingProduct = await _context.Products.FindAsync(product.ProductID);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(ProductMGMT));
                }

                if (image != null)
                {
                    var imageValidation = ValidateImage(image);
                    if (!imageValidation.IsValid)
                    {
                        ModelState.AddModelError("Image", imageValidation.ErrorMessage);
                        PrepareViewBagForError();
                        return View("ProductMGMT", await GetProductsForView());
                    }

                    // Delete old image if it exists
                    await DeleteExistingImage(existingProduct.ImageURL);

                    // Save new image
                    var imageResult = await SaveImage(image);
                    if (!imageResult.Success)
                    {
                        ModelState.AddModelError("Image", imageResult.ErrorMessage);
                        PrepareViewBagForError();
                        return View("ProductMGMT", await GetProductsForView());
                    }

                    existingProduct.ImageURL = imageResult.ImagePath;
                }

                // Update product properties
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
                TempData["Error"] = "Error updating product. Please try again.";
                return RedirectToAction(nameof(ProductMGMT));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(ProductMGMT));
                }

                // Delete image if it exists
                await DeleteExistingImage(product.ImageURL);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction(nameof(ProductMGMT));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                TempData["Error"] = "Error deleting product. Please try again.";
                return RedirectToAction(nameof(ProductMGMT));
            }
        }

        // Helper Methods
        private async Task<List<Product>> GetProductsForView()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        private async void PrepareViewBagForError()
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        private (bool IsValid, string ErrorMessage) ValidateImage(IFormFile image)
        {
            if (image.Length > MaxImageSize)
            {
                return (false, "Image size must be less than 5MB.");
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, "Invalid image format. Allowed formats are: jpg, jpeg, png, gif.");
            }

            return (true, string.Empty);
        }

        private async Task<(bool Success, string ImagePath, string ErrorMessage)> SaveImage(IFormFile image)
        {
            try
            {
                var uniqueFileName = GetUniqueFileName(image.FileName);
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return (true, $"/images/products/{uniqueFileName}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return (false, string.Empty, "Error saving image. Please try again.");
            }
        }

        private async Task DeleteExistingImage(string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        await Task.Run(() => System.IO.File.Delete(imagePath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting image: {ImagePath}", imagePath);
                        // Continue execution even if image deletion fails
                    }
                }
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                    + "_" + Guid.NewGuid().ToString("N").Substring(0, 8)
                    + Path.GetExtension(fileName).ToLowerInvariant();
        }
    }
}