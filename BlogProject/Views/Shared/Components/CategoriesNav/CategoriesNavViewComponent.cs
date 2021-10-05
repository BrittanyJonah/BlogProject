using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalBlog.Data;
using PersonalBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Views.Shared.Components.FooterTags
{
    public class CategoriesNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoriesNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await GetItemsAsync();
            ViewData["Tags"] = new SelectList(_context.Tags);

            return View(items);
        }
        private Task<List<Blog>> GetItemsAsync()
        {
            return _context.Blogs.OrderByDescending(p => p.Created).ToListAsync();
        }
    }
}
