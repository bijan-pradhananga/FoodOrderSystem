using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodOrderSystem.Areas.Identity.Data;

namespace FoodOrderSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; } // Primary Key for the Order

        [Required]
        public string UserId { get; set; } // Foreign Key to the ApplicationUser table

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Reference to the ApplicationUser who placed the order

        [Required]
        public decimal TotalAmount { get; set; } // Total amount for the order

        [Required]
        public OrderStatus Status { get; set; } // Status of the order (Pending, Paid, Shipped, etc.)

        public DateTime OrderDate { get; set; } = DateTime.Now; // Date the order was placed

        // Navigation property to the order items
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending, // Order has been placed but not yet paid
        Delivered, // Order has been delivered
        Cancelled // Order has been cancelled
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; } // Primary Key for the OrderItem

        [Required]
        public int OrderId { get; set; } // Foreign Key to the Order

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } // Reference to the associated Order

        [Required]
        public int ProductId { get; set; } // Foreign Key to the Product

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } // Reference to the Product being ordered

        [Required]
        public int Quantity { get; set; } // Quantity of the product in this order item

        [Required]
        public decimal Price { get; set; } // Price per unit of the product at the time of purchase
    }
}
