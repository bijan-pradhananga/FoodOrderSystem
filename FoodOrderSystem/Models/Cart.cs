using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace FoodOrderSystem.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string UserId { get; set; } 

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Change the foreign key reference to IdentityUser
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Ensure this references IdentityUser

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
