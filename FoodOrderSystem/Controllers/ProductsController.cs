using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodOrderSystem.Models;
using System.IO;
using System.Threading.Tasks;
using FoodOrderSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderSystem.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> Index(string sortOrder, int? categoryId)
        {
            // Get the list of categories for the dropdown filter
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Category_Name");

            // Start with all products and include categories
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply category filter if a categoryId is provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply sorting based on the sortOrder parameter
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    // No sorting applied
                    break;
            }

            return View(await products.ToListAsync());
        }


        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.Category)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); 
            }

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Display()
        {
            var products = await _context.Products.Include(p => p.Category).OrderByDescending(p => p.Id).ToListAsync();
            return View("~/Views/Admin/Products/Display.cshtml", products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Category_Name");
            return View("~/Views/Admin/Products/Create.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductDto productDto, IFormFile imageFile)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Image is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Category_Name", productDto.CategoryId);
                return View("~/Views/Admin/Products/Create.cshtml", productDto);
            }

            // Save image
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);

            // Ensure the directory exists
            if (!Directory.Exists(Path.Combine(environment.WebRootPath, "products")))
            {
                Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "products"));
            }

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // Save new product in database
            Product product = new Product()
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                Quantity = productDto.Quantity, // Map Quantity from ProductDto
                ImagePath = newFileName
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Display", "Products");
        }


        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Display", "Products");
            }

            // Create ProductDto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                Quantity = product.Quantity // Set Quantity in ProductDto
            };

            ViewData["id"] = product.Id;
            ViewData["image"] = product.ImagePath;
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Category_Name", productDto.CategoryId);
            return View("~/Views/Admin/Products/Edit.cshtml", productDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Display", "Products");
            }

            if (!ModelState.IsValid)
            {
                ViewData["id"] = product.Id;
                ViewData["image"] = product.ImagePath;
                ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Category_Name", productDto.CategoryId);
                return View("~/Views/Admin/Products/Edit.cshtml", productDto);
            }

            // Image update
            string newFileName = product.ImagePath;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile.FileName);
                string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);

                // Ensure directory exists
                if (!Directory.Exists(Path.Combine(environment.WebRootPath, "products")))
                {
                    Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "products"));
                }

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // Delete old image
                string oldImageFullPath = Path.Combine(environment.WebRootPath, "products", product.ImagePath);
                if (System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }

            // Update product in database
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId;
            product.Price = productDto.Price;
            product.Quantity = productDto.Quantity; // Update Quantity
            product.ImagePath = newFileName;

            _context.SaveChanges();
            return RedirectToAction("Display", "Products");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Display", "Products");
            }
            //delete old img
            string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImagePath;
            System.IO.File.Delete(oldImageFullPath);
            _context.Products.Remove(product);
            _context.SaveChanges();
            // Redirect to the index action after deletion
            return RedirectToAction("Display", "Products");
        }
    }
}