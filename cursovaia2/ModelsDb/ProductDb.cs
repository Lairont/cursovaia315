using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("products")]
    public class ProductDb
    {
        [Key]
        public int Id { get; set; }

        // Добавьте здесь поля соответствующие вашей таблице товаров
        // Пример:
        // [Column("name")]
        // public string Name { get; set; } = string.Empty;
        //
        // [Column("description")]
        // public string Description { get; set; } = string.Empty;
        //
        // [Column("price")]
        // public decimal Price { get; set; }
        //
        // [Column("category_id")]
        // public int CategoryId { get; set; }
        //
        // [ForeignKey("CategoryId")]
        // public CategoryDb Category { get; set; }
    }
}
