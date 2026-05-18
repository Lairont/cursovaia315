using cursovaia2.ModelsDb;

namespace cursovaia2.Models
{
    public class HomeViewModel
    {
        public List<CategoryDb> Categories { get; set; } = new();
        public List<ProductDb> PopularProducts { get; set; } = new();
        public Dictionary<int, ProductPriceInfo> ProductPrices { get; set; } = new();
        public HashSet<int> WishlistProductIds { get; set; } = new();
    }
}
