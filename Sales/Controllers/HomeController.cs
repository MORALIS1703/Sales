using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Data;
using Sales.Models.Category;

namespace Sales.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var categories = await _context.Categories.Select(c => new CategoryModel
            {
                Id = c.Id,
                Image = c.Image,
                Name = c.Name
            }).ToListAsync();

            return View(categories);
        }
    }
}
