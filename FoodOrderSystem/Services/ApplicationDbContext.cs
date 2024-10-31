using FoodOrderSystem.Areas.Identity.Data;
using FoodOrderSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderSystem.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { 
            
        }

        public DbSet<Product> Products { get; set; } 
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                 .Property(p => p.Price)
                 .HasPrecision(18, 2);
                        base.OnModelCreating(modelBuilder);

            // Define the relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete to remove products when category is deleted

            // Define the relationship between Cart and Product
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany() // Assuming a product can be in multiple carts
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading deletes here

            // Define the relationship between Cart and ApplicationUser
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany() // Assuming a user can have multiple carts (or items in one cart)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading deletes here

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        
  
    }
}
