using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("categories")]
    public class CategoryDb
    {
        [Key]
        public int Id { get; set; }

        // Добавьте здесь поля соответствующие вашей таблице категорий
        // Пример:
        // [Column("name")]
        // public string Name { get; set; } = string.Empty;
        //
        // [Column("icon")]
        // public string Icon { get; set; } = string.Empty;
    }
}
