using Microsoft.AspNetCore.Mvc;

namespace FoodOrderSystem.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
