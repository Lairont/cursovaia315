using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricing;

        public ProductController(ApplicationDbContext context, PricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        public IActionResult Catalog(int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string? sortBy, string? searchQuery)
        {
            try
            {
                var categories = _context.CategoriesDb.ToList();
                var brands = _context.Brands.ToList();

                var products = _context.ProductsDb
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Images)
                    .ToList();

                if (categoryId.HasValue)
                    products = products.Where(p => p.CategoryId == categoryId).ToList();

                if (brandId.HasValue)
                    products = products.Where(p => p.BrandId == brandId).ToList();

                if (minPrice.HasValue)
                    products = products.Where(p => p.Price >= minPrice).ToList();

                if (maxPrice.HasValue)
                    products = products.Where(p => p.Price <= maxPrice).ToList();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    products = products.Where(p =>
                        p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description != null && p.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        p.Sku.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                products = sortBy switch
                {
                    "price_asc" => products.OrderBy(p => p.Price).ToList(),
                    "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                    "name_asc" => products.OrderBy(p => p.Name).ToList(),
                    "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                    _ => products.OrderBy(p => p.Name).ToList()
                };

                var productIds = products.Select(p => p.Id).ToList();

                var viewModel = new CatalogViewModel
                {
                    Categories = categories,
                    Brands = brands,
                    Products = products,
                    ProductRatings = BuildRatingsDictionary(productIds),
                    ProductPrices = _pricing.GetProductPrices(products),
                    WishlistProductIds = GetWishlistIds(),
                    SelectedCategoryId = categoryId,
                    SelectedBrandId = brandId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SearchQuery = searchQuery,
                    SortBy = sortBy,
                    MinPriceAll = products.Any() ? products.Min(p => p.Price) : 0,
                    MaxPriceAll = products.Any() ? products.Max(p => p.Price) : 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при загрузке товаров: {ex.Message}";
                return View(new CatalogViewModel());
            }
        }

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
                    return NotFound();

                var rating = GetRatingSummary(id);
                int? userRating = null;
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    userRating = _context.Reviews
                        .Where(r => r.ProductId == id && r.CustomerId == userId.Value)
                        .Select(r => (int?)r.Rating)
                        .FirstOrDefault();
                }

                var similarProducts = _context.ProductsDb
                    .Where(p => p.IsActive && p.Id != id && p.CategoryId == product.CategoryId)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Images)
                    .Take(4)
                    .ToList();

                var wishlistIds = GetWishlistIds();

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    Rating = rating,
                    UserRating = userRating,
                    SimilarProducts = similarProducts,
                    SimilarRatings = BuildRatingsDictionary(similarProducts.Select(p => p.Id).ToList()),
                    Price = _pricing.GetProductPrice(product.Id, product.Price),
                    InWishlist = wishlistIds.Contains(id),
                    SimilarPrices = _pricing.GetProductPrices(similarProducts),
                    WishlistProductIds = wishlistIds
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при загрузке товара: {ex.Message}";
                return RedirectToAction(nameof(Catalog));
            }
        }

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
                        (p.Description != null && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (categoryId.HasValue)
                    products = products.Where(p => p.CategoryId == categoryId).ToList();

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

        private HashSet<int> GetWishlistIds()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return new HashSet<int>();

            return _context.Wishlists
                .Where(w => w.CustomerId == userId.Value)
                .Select(w => w.ProductId)
                .ToHashSet();
        }

        private Dictionary<int, RatingSummary> BuildRatingsDictionary(List<int> productIds)
        {
            if (!productIds.Any())
                return new Dictionary<int, RatingSummary>();

            return _context.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToDictionary(
                    x => x.ProductId,
                    x => new RatingSummary
                    {
                        Average = Math.Round(x.Average, 1),
                        Count = x.Count
                    });
        }

        private RatingSummary GetRatingSummary(int productId)
        {
            var dict = BuildRatingsDictionary(new List<int> { productId });
            return dict.GetValueOrDefault(productId, new RatingSummary());
        }
    }
}
