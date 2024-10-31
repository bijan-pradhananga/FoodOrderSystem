using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderSystem.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Category_Name { get; set; }

        [NotMapped] // Prevents EF from requiring this field
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
