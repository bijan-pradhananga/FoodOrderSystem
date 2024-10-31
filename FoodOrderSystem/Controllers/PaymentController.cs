using FoodOrderSystem.Areas.Identity.Data;
using FoodOrderSystem.Models;
using FoodOrderSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FoodOrderSystem.Controllers
{
   
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly EsewaPaymentService _esewaPaymentService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(EsewaPaymentService esewaPaymentService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _esewaPaymentService = esewaPaymentService;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, Order order)
        {
            var userId = _userManager.GetUserId(User);

            // Retrieve cart items for the user
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
            {
				return RedirectToAction("Failure", "Payment");
			}

            // Create the order object
            order.UserId = userId;
            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.Pending;
            order.TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);
            order.OrderItems = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                Price = c.Product.Price
            }).ToList();

            // Decrease the quantity of each product in stock
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity; 
                                                       
                    if (product.Quantity < 0)
                    {
                        // Handle out-of-stock scenario (optional)
                        product.Quantity = 0; // Set to zero or handle appropriately
                    }
                }
            }

            // Save to database
            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems); // Clear the cart after order
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { orderId = order.Id });
        }


        public IActionResult Success(int orderId)
        {
            // Set a custom message for the order confirmation
            ViewBag.Message = $"Your order #{orderId} will be delivered in a few hours. Please have the cash ready.";

            return View();
        }

        public IActionResult Failure()
        {
            return View(); // Display failure view
        }
    }
}
