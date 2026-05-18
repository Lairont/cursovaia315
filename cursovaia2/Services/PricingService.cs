using cursovaia2.Data;
using cursovaia2.Models;
using cursovaia2.ModelsDb;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Services
{
    public class PricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ProductPriceInfo GetProductPrice(int productId, decimal basePrice)
        {
            var discounts = GetActiveDiscountsForProduct(productId);
            var final = basePrice;
            foreach (var d in discounts)
                final = ApplyDiscount(final, d);

            return new ProductPriceInfo
            {
                OriginalPrice = basePrice,
                FinalPrice = Math.Max(0, final)
            };
        }

        public Dictionary<int, ProductPriceInfo> GetProductPrices(IEnumerable<ProductDb> products)
        {
            var list = products.ToList();
            var ids = list.Select(p => p.Id).ToList();
            var productDiscounts = _context.ProductDiscounts
                .Where(pd => ids.Contains(pd.ProductId))
                .Include(pd => pd.Discount)
                .ToList()
                .Where(pd => pd.Discount != null && IsDiscountActive(pd.Discount))
                .GroupBy(pd => pd.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Discount!).ToList());

            var result = new Dictionary<int, ProductPriceInfo>();
            foreach (var p in list)
            {
                var final = p.Price;
                if (productDiscounts.TryGetValue(p.Id, out var discounts))
                {
                    foreach (var d in discounts)
                        final = ApplyDiscount(final, d);
                }
                result[p.Id] = new ProductPriceInfo
                {
                    OriginalPrice = p.Price,
                    FinalPrice = Math.Max(0, final)
                };
            }
            return result;
        }

        public PromoApplyResult? ValidatePromoCode(string? code, decimal orderSubtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var promo = _context.PromoCodes
                .Include(p => p.Discount)
                .FirstOrDefault(p => p.Code.ToLower() == code.Trim().ToLower());

            if (promo?.Discount == null)
                return PromoApplyResult.Fail("Промокод не найден");

            if (promo.UsageLimit.HasValue && promo.TimesUsed >= promo.UsageLimit.Value)
                return PromoApplyResult.Fail("Промокод исчерпан");

            if (!IsDiscountActive(promo.Discount))
                return PromoApplyResult.Fail("Промокод недействителен");

            var amount = CalculateDiscountAmount(orderSubtotal, promo.Discount);
            return PromoApplyResult.Ok(promo.Id, promo.DiscountId, amount, orderSubtotal - amount);
        }

        public decimal CalculateDiscountAmount(decimal subtotal, DiscountDb discount)
        {
            if (subtotal <= 0) return 0;
            return NormalizeType(discount.Type) switch
            {
                "percent" => Math.Round(subtotal * discount.Value / 100m, 2),
                "fixed" => Math.Min(discount.Value, subtotal),
                _ => 0
            };
        }

        private List<DiscountDb> GetActiveDiscountsForProduct(int productId)
        {
            return _context.ProductDiscounts
                .Where(pd => pd.ProductId == productId)
                .Include(pd => pd.Discount)
                .Select(pd => pd.Discount!)
                .ToList()
                .Where(d => d != null && IsDiscountActive(d))
                .ToList();
        }

        private static bool IsDiscountActive(DiscountDb d)
        {
            var now = DateTime.UtcNow;
            if (d.StartDate.HasValue && d.StartDate.Value.ToUniversalTime() > now) return false;
            if (d.EndDate.HasValue && d.EndDate.Value.ToUniversalTime() < now) return false;
            return true;
        }

        private static decimal ApplyDiscount(decimal price, DiscountDb d)
        {
            return NormalizeType(d.Type) switch
            {
                "percent" => price - price * d.Value / 100m,
                "fixed" => price - d.Value,
                _ => price
            };
        }

        private static string NormalizeType(string? type)
        {
            return (type ?? "").Trim().ToLowerInvariant() switch
            {
                "percent" or "percentage" or "%" => "percent",
                "fixed" or "amount" or "rub" => "fixed",
                _ => (type ?? "").Trim().ToLowerInvariant()
            };
        }
    }

    public class PromoApplyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int PromoCodeId { get; set; }
        public int DiscountId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NewTotal { get; set; }

        public static PromoApplyResult Ok(int promoId, int discountId, decimal amount, decimal newTotal) =>
            new() { Success = true, PromoCodeId = promoId, DiscountId = discountId, DiscountAmount = amount, NewTotal = newTotal, Message = "Промокод применён" };

        public static PromoApplyResult Fail(string msg) =>
            new() { Success = false, Message = msg };
    }
}
