using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("product_attributes")]
    public class ProductAttributeDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        [Required]
        public int ProductId { get; set; }

        [Column("key")]
        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        [Required]
        [StringLength(100)]
        public string Value { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual ProductDb? Product { get; set; }
    }
}
