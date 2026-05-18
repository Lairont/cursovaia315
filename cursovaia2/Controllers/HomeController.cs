using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace cursovaia2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricing;

        public HomeController(ApplicationDbContext context, PricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            var popularProducts = _context.ProductsDb
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            var wishlistIds = userId.HasValue
                ? _context.Wishlists.Where(w => w.CustomerId == userId.Value).Select(w => w.ProductId).ToHashSet()
                : new HashSet<int>();

            var model = new HomeViewModel
            {
                Categories = _context.CategoriesDb.OrderBy(c => c.Name).Take(6).ToList(),
                PopularProducts = popularProducts,
                ProductPrices = _pricing.GetProductPrices(popularProducts),
                WishlistProductIds = wishlistIds
            };

            return View(model);
        }

        public IActionResult About() => View();

        public IActionResult Contacts() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
