using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("order_items")]
    public class OrderItemDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        [Required]
        public int OrderId { get; set; }

        [Column("product_id")]
        [Required]
        public int ProductId { get; set; }

        [Column("quantity")]
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Column("price")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual OrderDb? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual ProductDb? Product { get; set; }
    }
}
