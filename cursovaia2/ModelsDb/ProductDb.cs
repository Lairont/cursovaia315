using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("products")]
    public class ProductDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Column("sku")]
        [Required]
        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("brand_id")]
        public int? BrandId { get; set; }

        [Column("price")]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Column("cost_price")]
        [Range(0, double.MaxValue)]
        public decimal? CostPrice { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual CategoryDb? Category { get; set; }

        [ForeignKey("BrandId")]
        public virtual BrandDb? Brand { get; set; }

        public virtual ICollection<ProductImageDb> Images { get; set; } = new List<ProductImageDb>();
        public virtual ICollection<ProductAttributeDb> Attributes { get; set; } = new List<ProductAttributeDb>();
        public virtual ICollection<OrderItemDb> OrderItems { get; set; } = new List<OrderItemDb>();
        public virtual ICollection<CartItemDb> CartItems { get; set; } = new List<CartItemDb>();
        public virtual ICollection<ReviewDb> Reviews { get; set; } = new List<ReviewDb>();
        public virtual ICollection<WishlistDb> Wishlists { get; set; } = new List<WishlistDb>();
        public virtual ICollection<ProductDiscountDb> Discounts { get; set; } = new List<ProductDiscountDb>();
    }
}
