using FoodOrderSystem.Areas.Identity.Data;
using FoodOrderSystem.Models;
using FoodOrderSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace FoodOrderSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id) // Sort by a field, like Id or CreatedDate
                .Take(4) // Limit to 4 products
                .ToListAsync();

            return View(products);
        }


        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Home2()
        {
            return View();
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect(returnUrl ?? Url.Content("/"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
