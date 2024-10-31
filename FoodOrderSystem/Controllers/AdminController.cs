using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using FoodOrderSystem.Areas.Identity.Data; // Adjust this path as needed
using FoodOrderSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FoodOrderSystem.Services;

public class AdminController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // GET: /Admin/Login
    [HttpGet]
    public IActionResult Login(string returnUrl = "~/Admin/Login")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Admin/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model, string returnUrl = null)
    {
        // If returnUrl is null, default to the admin index page.
        returnUrl = returnUrl ?? Url.Content("~/Admin/Index");
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Successful login: redirect to the Admin Index page or the returnUrl.
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    // Login failed, but user is found and is admin. Add login failure message.
                    ModelState.AddModelError("FailedLogin", "Invalid login attempt.");
                }
            }
        }

        // If we got this far, something failed, redisplay the form with validation errors.
        return View("Login", model);
    }




    // Optional: Logout action
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl ?? Url.Content("~/Admin/Login"));
    }

    // Home action for admin
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        // Fetch the counts for products, categories, and orders
        var productCount = await _context.Products.CountAsync();
        var categoryCount = await _context.Categories.CountAsync();
        var orderCount = await _context.Orders.CountAsync();

        // Pass counts to the view using ViewBag or a ViewModel
        ViewBag.ProductCount = productCount;
        ViewBag.CategoryCount = categoryCount;
        ViewBag.OrderCount = orderCount;

        return View();
    }

}
