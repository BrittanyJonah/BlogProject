using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBlog.Data;
using PersonalBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalBlog.Views.Shared.Components.FooterTags
{
    public class SideNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SideNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await GetItemsAsync();

            return View(items);
        }
        private Task<List<Tag>> GetItemsAsync()
        {
            return _context.Tags.AsQueryable().ToListAsync();
        }
    }
}
