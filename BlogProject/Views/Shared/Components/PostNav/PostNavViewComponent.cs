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
    public class PostNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public PostNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await GetItemsAsync();
            ViewData["Tags"] = _context.Tags.AsQueryable();

            return View(items);
        }
        private Task<List<Post>> GetItemsAsync()
        {
            return _context.Posts.Include(p => p.Blog).OrderByDescending(p => p.PageViews).ToListAsync();
        }
    }
}
