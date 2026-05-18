using cursovaia2.Data;
using cursovaia2.ModelsDb;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricing;

        public WishlistController(ApplicationDbContext context, PricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var items = _context.Wishlists
                .Where(w => w.CustomerId == userId.Value)
                .Include(w => w.Product)
                .ThenInclude(p => p!.Images)
                .Include(w => w.Product)
                .ThenInclude(p => p!.Category)
                .OrderByDescending(w => w.CreatedAt)
                .ToList();

            ViewBag.Prices = _pricing.GetProductPrices(
                items.Where(i => i.Product != null).Select(i => i.Product!));

            return View(items);
        }

        [HttpPost]
        public IActionResult Toggle([FromBody] WishlistRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Войдите в аккаунт" });

            var product = _context.ProductsDb.FirstOrDefault(p => p.Id == request.ProductId && p.IsActive);
            if (product == null)
                return Json(new { success = false, message = "Товар не найден" });

            var existing = _context.Wishlists
                .FirstOrDefault(w => w.CustomerId == userId.Value && w.ProductId == request.ProductId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                _context.SaveChanges();
                return Json(new { success = true, inWishlist = false, message = "Удалено из избранного" });
            }

            _context.Wishlists.Add(new WishlistDb
            {
                CustomerId = userId.Value,
                ProductId = request.ProductId,
                CreatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
            return Json(new { success = true, inWishlist = true, message = "Добавлено в избранное" });
        }
    }

    public class WishlistRequest
    {
        public int ProductId { get; set; }
    }
}
