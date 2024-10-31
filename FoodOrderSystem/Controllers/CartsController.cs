using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderSystem.Data;
using FoodOrderSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using FoodOrderSystem.Services;
using FoodOrderSystem.Areas.Identity.Data;
using System.Diagnostics;

namespace FoodOrderSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            // Retrieve the product to check its stock quantity
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                // Return an error if the product doesn't exist
                return NotFound("Product not found.");
            }

            // Retrieve the existing cart item if it exists
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                // Check if adding the requested quantity exceeds available stock
                if (existingCartItem.Quantity + quantity > product.Quantity)
                {
                    // If it exceeds, set the quantity to the max available and return an error message
                    existingCartItem.Quantity = product.Quantity;
                    TempData["ErrorMessage"] = "Requested quantity exceeds available stock. Quantity set to maximum available.";
                }
                else
                {
                    // Otherwise, update the cart item quantity
                    existingCartItem.Quantity += quantity;
                }

                _context.Update(existingCartItem);
            }
            else
            {
                // Ensure the requested quantity is within the available stock for new cart items
                if (quantity > product.Quantity)
                {
                    quantity = product.Quantity;
                    TempData["ErrorMessage"] = "Requested quantity exceeds available stock. Quantity set to maximum available.";
                }

                var newCartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _context.Carts.AddAsync(newCartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }




        // GET: Categories/Delete/5
        public async Task<IActionResult> Remove(int? id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return RedirectToAction("Index");
            }
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
