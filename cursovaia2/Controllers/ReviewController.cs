using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SubmitRating([FromBody] RatingRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Войдите в аккаунт, чтобы поставить оценку" });

            if (request.Rating < 1 || request.Rating > 5)
                return Json(new { success = false, message = "Оценка должна быть от 1 до 5" });

            var product = _context.ProductsDb.FirstOrDefault(p => p.Id == request.ProductId && p.IsActive);
            if (product == null)
                return Json(new { success = false, message = "Товар не найден" });

            var review = _context.Reviews
                .FirstOrDefault(r => r.ProductId == request.ProductId && r.CustomerId == userId.Value);

            if (review != null)
            {
                review.Rating = request.Rating;
                review.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Reviews.Add(new ReviewDb
                {
                    ProductId = request.ProductId,
                    CustomerId = userId.Value,
                    Rating = request.Rating,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            var stats = GetRatingSummary(request.ProductId);
            return Json(new
            {
                success = true,
                message = "Оценка сохранена",
                userRating = request.Rating,
                average = stats.Average,
                count = stats.Count,
                hasRatings = stats.HasRatings
            });
        }

        [HttpGet]
        public IActionResult GetProductRating(int productId)
        {
            var stats = GetRatingSummary(productId);
            var userId = HttpContext.Session.GetInt32("UserId");
            int? userRating = null;

            if (userId.HasValue)
            {
                userRating = _context.Reviews
                    .Where(r => r.ProductId == productId && r.CustomerId == userId.Value)
                    .Select(r => (int?)r.Rating)
                    .FirstOrDefault();
            }

            return Json(new
            {
                average = stats.Average,
                count = stats.Count,
                hasRatings = stats.HasRatings,
                userRating
            });
        }

        private RatingSummary GetRatingSummary(int productId)
        {
            var reviews = _context.Reviews.Where(r => r.ProductId == productId).ToList();
            if (!reviews.Any())
                return new RatingSummary();

            return new RatingSummary
            {
                Average = Math.Round(reviews.Average(r => r.Rating), 1),
                Count = reviews.Count
            };
        }
    }

    public class RatingRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
    }
}
