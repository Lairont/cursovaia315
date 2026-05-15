using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("categories")]
    public class CategoryDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("parent_id")]
        public int? ParentId { get; set; }

        // Navigation properties
        [ForeignKey("ParentId")]
        public virtual CategoryDb? Parent { get; set; }

        public virtual ICollection<CategoryDb> Children { get; set; } = new List<CategoryDb>();
        public virtual ICollection<ProductDb> Products { get; set; } = new List<ProductDb>();
    }
}
