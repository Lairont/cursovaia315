using cursovaia2.ModelsDb;

namespace cursovaia2.Models
{
    public class RatingSummary
    {
        public double Average { get; set; }
        public int Count { get; set; }
        public bool HasRatings => Count > 0;
    }

    public class CatalogViewModel
    {
        public List<CategoryDb> Categories { get; set; } = new();
        public List<BrandDb> Brands { get; set; } = new();
        public List<ProductDb> Products { get; set; } = new();
        public Dictionary<int, RatingSummary> ProductRatings { get; set; } = new();
        public Dictionary<int, ProductPriceInfo> ProductPrices { get; set; } = new();
        public HashSet<int> WishlistProductIds { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public int? SelectedBrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }
        public decimal MinPriceAll { get; set; }
        public decimal MaxPriceAll { get; set; }
    }

    public class ProductDetailViewModel
    {
        public ProductDb Product { get; set; } = null!;
        public RatingSummary Rating { get; set; } = new();
        public int? UserRating { get; set; }
        public List<ProductDb> SimilarProducts { get; set; } = new();
        public Dictionary<int, RatingSummary> SimilarRatings { get; set; } = new();
        public ProductPriceInfo Price { get; set; } = new();
        public bool InWishlist { get; set; }
        public Dictionary<int, ProductPriceInfo> SimilarPrices { get; set; } = new();
        public HashSet<int> WishlistProductIds { get; set; } = new();
    }
}
