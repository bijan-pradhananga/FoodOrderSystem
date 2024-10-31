using FoodOrderSystem.Areas.Identity.Data;
using FoodOrderSystem.Models;
using FoodOrderSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoodOrderSystem.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EsewaPaymentService _esewaPaymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, EsewaPaymentService esewaPaymentService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _esewaPaymentService = esewaPaymentService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate) // Order by latest first
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Display()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                 .OrderByDescending(o => o.OrderDate) // Order by latest first
                .ToListAsync();

            return View("~/Views/Admin/Orders/Display.cshtml", orders);
        }

        // Display details of a specific order
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || order.UserId != User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value)
            {
                return NotFound();
            }

            return View(order);
        }

		// Cancel Order
		[HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelOrder(int orderId)
		{
			var order = await _context.Orders
				.Include(o => o.OrderItems) // Include OrderItems to access product information
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
			{
				return NotFound();
			}

			// Increase the quantity of each product in the order
			foreach (var item in order.OrderItems)
			{
				var product = await _context.Products.FindAsync(item.ProductId);
				if (product != null)
				{
					product.Quantity += item.Quantity; // Increase product quantity by the ordered quantity
					_context.Products.Update(product); // Update product in the context
				}
			}

			order.Status = OrderStatus.Cancelled; // Change order status
			_context.Orders.Update(order); // Update the order status
			await _context.SaveChangesAsync(); // Save changes to the database

			TempData["Message"] = "Order has been cancelled successfully.";
			return RedirectToAction("Index", "Orders"); // Redirect to the orders list page
		}


		// Confirm Order
		[HttpPost]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            order.Status = OrderStatus.Delivered;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order has been confirmed as delivered.";
            return RedirectToAction("Display", "Orders");
        }
            // Create an order from cart items and initiate eSewa payment
            [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var cartItems = await _context.Carts
                 .Include(c => c.Product)
                 .Where(c => c.UserId == userId)
                 .ToListAsync();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Carts");
            }
            decimal totalAmount = 0;

            foreach (var c in cartItems)
            {
                if (c.Product == null)
                {
                    return RedirectToAction("Index", "Carts");
                }

                totalAmount += c.Product.Price * c.Quantity;
            }
            ViewBag.FullName = user.FullName;
            ViewBag.Address = user.Address;
            ViewBag.Email = user.Email;
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = totalAmount;
            return View(); // Pass the order to the checkout view
        }


    }
}
