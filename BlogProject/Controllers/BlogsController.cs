using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalBlog.Data;
using PersonalBlog.Models;
using PersonalBlog.Services;
using X.PagedList;

namespace PersonalBlog.Controllers
{
    public class BlogsController : Controller
    {
        //Access to the database via Constructor Dependancy Injection.
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly ISlugService _slugService;
        private readonly UserManager<BlogUser> _userManager;


        public BlogsController(ApplicationDbContext context, IImageService imageService, UserManager<BlogUser> userManager, ISlugService slugService)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _slugService = slugService;
        }

        // GET: Blogs
        public async Task<IActionResult> Index(int? page)
        {
            //Queryable 
            //goes out to database, talks to blog table and bloguser, takes data and executes and turns into 
            //an async list and feeds it into the view
            var pageNumber = page ?? 1;
            var pageSize = 4;


            var applicationDbContext = await _context.Blogs
                .Include(b => b.BlogUser)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            //render view
            return View(applicationDbContext);
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(string slug, int? page)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var pageNumber = page ?? 1;
            var pageSize = 4;

            var blog = await _context.Blogs
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            ViewData["Blogs"] = _context.Blogs.
                Include(b => b.Posts).AsQueryable();

            ViewData["Posts"] = await _context.Posts.Where(p => p.BlogId.Equals(blog.Id)).ToPagedListAsync(pageNumber, pageSize);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blogs/Create
        [Authorize(Roles = "Admin")] //must be logged in to access view
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Image")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                //Sets date created
                blog.Created = DateTime.Now;
                //Take in user info
                blog.BlogUserId = _userManager.GetUserId(User);
                //Image
                blog.ImageData = await _imageService.EncodeImageAsync(blog.Image);
                blog.ContentType = _imageService.ContentType(blog.Image);

                var slug = _slugService.urlFriendly(blog.Name);
                if (!_slugService.isUnique(slug))
                {
                    //Add a model state error and return user back to the create view
                    ModelState.AddModelError("Title", "The title you provided cannot be used as it results in a name collision.");

                    return View(blog);
                }

                //Create variable to detect if an error has occured
                var validationError = false;


                //Detect empty slugs
                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError("", "The title can not be blank.");
                    validationError = true;
                }

                //Detect incoming duplicate slugs
                else if (!_slugService.isUnique(slug))
                {
                    ModelState.AddModelError("Title", "The title you provided could not be used as it results in a duplicate slug.");
                    validationError = true;
                }

                if (validationError)
                {
                    return View(blog);
                }

                blog.Slug = slug;

                //Adding to the database
                _context.Add(blog);
                await _context.SaveChangesAsync();//stores in database

                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }

        // GET: Blogs/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Blog blog, IFormFile newImage)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newBlog = await _context.Blogs.FindAsync(blog.Id);

                    newBlog.Updated = DateTime.Now;

                    //update if changes were made
                    if (newBlog.Name != blog.Name)
                    {
                        newBlog.Name = blog.Name;
                    }

                    if (newBlog.Description != blog.Description)
                    {
                        newBlog.Description = blog.Description;
                    }

                    //if uploaded a new image then update image
                    if (newImage is not null)
                    {
                        newBlog.ImageData = await _imageService.EncodeImageAsync(newImage);
                    }
                    //saved to DB
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }

        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
