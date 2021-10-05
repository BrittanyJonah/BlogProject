using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalBlog.Data;
using PersonalBlog.Models;
using PersonalBlog.Services;
using PersonalBlog.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBlogEmailSender _emailSender;
        private readonly IImageService _imageService;
        private readonly ISlugService _slugService;
        private readonly UserManager<BlogUser> _userManager;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IBlogEmailSender emailSender, IImageService imageService, ISlugService slugService, UserManager<BlogUser> userManager)
        {
            _logger = logger;
            _context = context;
            _emailSender = emailSender;
            _imageService = imageService;
            _slugService = slugService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //var sponsored = _context.Blogs.Where(b => b.Posts.Any(p => p.PostLocation == Enums.PostLocation.Sponsored));

            var posts = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .ToListAsync();

            ViewData["Blogs"] = _context.Blogs.AsQueryable();
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            ViewData["Highlights"] = _context.Posts.OrderByDescending(p => p.PageViews);
            ViewData["Sponsored"] = _context.Posts.OrderBy(p => p.PageViews);
            ViewData["Newest"] = _context.Posts.OrderByDescending(p => p.Created);
            //Enum Viewbags
            ViewData["Large"] = await _context.Posts.FirstOrDefaultAsync(p => p.PostLocation == Enums.PostLocation.Large);
            ViewData["Top"] = await _context.Posts.FirstOrDefaultAsync(p => p.PostLocation == Enums.PostLocation.Top);
            ViewData["BottomLeft"] = await _context.Posts.FirstOrDefaultAsync(p => p.PostLocation == Enums.PostLocation.BottomLeft);
            ViewData["BottomRight"] = await _context.Posts.FirstOrDefaultAsync(p => p.PostLocation == Enums.PostLocation.BottomRight);
            return View(posts);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            //email
            model.Message = model.Message;
            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);

            //redirect
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
