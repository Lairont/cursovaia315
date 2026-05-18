namespace cursovaia2.Models
{
    public class ProductPriceInfo
    {
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public bool HasDiscount => FinalPrice < OriginalPrice;
        public int? DiscountPercent => HasDiscount && OriginalPrice > 0
            ? (int)Math.Round((1 - FinalPrice / OriginalPrice) * 100)
            : null;
    }
}
