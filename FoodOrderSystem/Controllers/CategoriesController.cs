using Microsoft.AspNetCore.Mvc;
using FoodOrderSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using FoodOrderSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View("~/Views/Admin/Categories/Index.cshtml", categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/Categories/Create.cshtml");
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Category_Name")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Categories");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
            }
            return View("~/Views/Admin/Categories/Create.cshtml", category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        { 
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return RedirectToAction("Index", "Categories");
            }
            return View("~/Views/Admin/Categories/Edit.cshtml", category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Category_Name")] Category category)
        {
            if (id != category.Id)
            {
                return RedirectToAction("Index", "Categories");
            }

            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Categories");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return RedirectToAction("Index", "Categories");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Categories");
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
