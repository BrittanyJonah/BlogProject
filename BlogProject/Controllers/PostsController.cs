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
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly ISlugService _slugService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;

        public PostsController(ApplicationDbContext context, IImageService imageService, ISlugService slugService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _imageService = imageService;
            _slugService = slugService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }

        // GET : Search
        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["searchTerm"] = searchTerm ?? "";

            var pageNum = page ?? 1;
            var pageSize = 5;

            var posts = _blogSearchService
                .Search(searchTerm)
                .Include(p => p.Blog)
                .Include(p => p.BlogUser);
            ViewData["resultsCount"] = posts.Count();

            return View(await posts.ToPagedListAsync(pageNum, pageSize));
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 12;

            var applicationDbContext = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .OrderByDescending(p => p.Created)
                .ToPagedListAsync(pageNumber, pageSize); //includes trigger "eager loading"

            return View(applicationDbContext);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.Blog.Posts)  //Include related posts
                .Include(p => p.BlogUser)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(c => c.BlogUser)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            
            if (post == null)
            {
                return NotFound();
            }


            //Previous/Next navigation ordered by posted date
            List<Post> orderedPosts = await _context.Posts.OrderBy(p => p.Created).ToListAsync();
            int iteration = 0;
            foreach (var item in orderedPosts)
            {
                //Determine the current post within the ordered list
                if (item.Id == post.Id)
                {
                    //Next will not apply on last item
                    if (iteration < (orderedPosts.Count - 1))
                    {
                        ViewData["nextPost"] = orderedPosts[iteration + 1];
                    }
                    //Previous will not apply on first item
                    if (iteration > 0)
                    {
                        ViewData["prevPost"] = orderedPosts[iteration - 1];
                    }
                }
                iteration++;
            }

            post.PageViews++;
            await _context.SaveChangesAsync();

            return View(post);
        }

        // GET: Posts/Create
        [Authorize(Roles = "Admin")] //must be logged in to access view
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,PostLocation,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                //records created date
                post.Created = DateTime.Now;

                //Take in user info
                var authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                //image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);

                //create the slug and determine if it is unique
                var slug = _slugService.urlFriendly(post.Title);

                //store if error occured
                var validationError = false;

                if (string.IsNullOrEmpty(slug))
                {
                    validationError = true;
                    //add a model state error and return user back to the create view
                    ModelState.AddModelError("Title", "The Title cannot be left blank.");

                }

                //detect incoming duplicates
                else if(!_slugService.isUnique(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it already exists.");
                }

                if (validationError)
                {
                    //rebuilds  inputted tags so user does not have to repeat
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
                    return View(post);
                }

                //use slug service
                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();

                //Loop over incoming list of tags
                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogUserId = authorId,
                        Text = tagText
                    });
                }

                await _context.SaveChangesAsync();
                //returns to list of posts
                return RedirectToAction(nameof(Index));
            }

            //refills selectlist with blogs when post fails to send data to database
            //params: where, where it sends selection TO, what selections are displayed
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Content,PostLocation,Image")] Post post, 
                                                       IFormFile newImage, List<string> tagValues, string blogSlug, string postSlug)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //access to original post
                    var originalPost = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == post.Id);

                    //Updates Updated Dates Datetime
                    originalPost.Updated = DateTime.Now;

                    //updates new originalPost
                    originalPost.Title = post.Title;
                    originalPost.Abstract = post.Abstract;
                    originalPost.Content = post.Content;
                    originalPost.PostLocation = post.PostLocation;

                    var newSlug = _slugService.urlFriendly(post.Title);
                    if (newSlug != originalPost.Slug)
                    {
                        if (_slugService.isUnique(newSlug))
                        {
                            originalPost.Title = post.Title;
                            originalPost.Slug = newSlug;
                            postSlug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "Cannot have duplicate Title.");
                            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", originalPost.BlogId);
                            ViewData["TagValues"] = string.Join(",", originalPost.Tags.Select(t => t.Text));
                            return RedirectToAction("Details");
                        }
                    }

                    if (newImage is not null)
                    {
                        originalPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        originalPost.ContentType = _imageService.ContentType(newImage);
                    }

                    //remove all tags previously associated with this post
                    _context.Tags.RemoveRange(originalPost.Tags);

                    //add in new tags from form
                    foreach (var tagText in tagValues)
                    {
                        _context.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogUserId = originalPost.BlogUserId,
                            Text = tagText
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { blog = blogSlug, slug = postSlug });
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return RedirectToAction("Details", new { blog = blogSlug, slug = postSlug });
        }

        // POST: Posts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
