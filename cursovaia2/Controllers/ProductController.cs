using cursovaia2.Data;
using cursovaia2.ModelsDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product/Catalog
        public IActionResult Catalog(int? categoryId)
        {
            try
            {
                var categories = _context.CategoriesDb.ToList();
                var products = _context.ProductsDb
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .ToList();

                // Фильтруем по категории если указана
                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId).ToList();
                }

                var viewModel = new
                {
                    Categories = categories,
                    Products = products,
                    SelectedCategoryId = categoryId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при загрузке товаров: {ex.Message}";
                return View();
            }
        }

        // GET: Product/Detail/5
        public IActionResult Detail(int id)
        {
            try
            {
                var product = _context.ProductsDb
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Images)
                    .Include(p => p.Attributes)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                    return NotFound("Товар не найден");

                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при загрузке товара: {ex.Message}";
                return View();
            }
        }

        // GET: Product/SearchJson
        [HttpGet]
        public IActionResult SearchJson(string? query, int? categoryId)
        {
            try
            {
                var products = _context.ProductsDb
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .ToList();

                if (!string.IsNullOrEmpty(query))
                {
                    products = products.Where(p =>
                        p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId).ToList();
                }

                var result = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Sku,
                    Category = p.Category?.Name,
                    Brand = p.Brand?.Name
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
