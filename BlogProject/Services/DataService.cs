using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalBlog.Data;
using PersonalBlog.Enums;
using PersonalBlog.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Services
{
    public class DataService
    {
        //communicates with Database
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;

        //constructor
        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<BlogUser> userManager, IConfiguration configuration, IImageService imageService)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _imageService = imageService;
        }

        /// <summary>
        /// Method wrapper to call private methods to get work done
        /// </summary>
        public async Task ManageDataAsync()
        {
            //Create DB from the Migrations
            await _dbContext.Database.MigrateAsync(); //equivalent to running update database command locally

            //Seed Roles into the Database
            await SeedRolesAsync();

            //Seed Users into the Database
            await SeedUserAsync();
        }

        /// <summary>
        /// Seed roles into the database
        /// </summary>
        /// <returns></returns>
        private async Task SeedRolesAsync()
        {
            //If there are already roles in DB, do nothing.
            if (_dbContext.Roles.Any()) return;

            //else create roles
            foreach (var role in Enum.GetNames(typeof(BlogRole)))
            {
                //Use Role Manager to Create Roles
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        /// <summary>
        /// Seed Users into the Database
        /// </summary>
        private async Task SeedUserAsync()
        {
            //If there are already Users in DB, do nothing.
            if (_dbContext.Users.Any()) { return; }

            //else creates new Instance of BlogUser
            else
            {
                //=====ADMIN=======================================================================

                var adminUser = new BlogUser()
                {
                    Email = "admin@example.com",
                    UserName = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "Admin",
                    PhoneNumber = "",
                    ImageData = await _imageService.EncodeImageAsync(_configuration["DefaultUserImage"]),
                    ContentType = Path.GetExtension(_configuration["DefaultUserImage"]),
                    EmailConfirmed = true
                };

                //Use UserManager to Create user defined by 
                await _userManager.CreateAsync(adminUser, "yourPassword!");

                //Add Admin Role to new User
                await _userManager.AddToRoleAsync(adminUser, BlogRole.Admin.ToString());

                //======MODERATOR======================================================================

                var modUser = new BlogUser()
                {
                    Email = "mod@example.com",
                    UserName = "mod@example.com",
                    FirstName = "Moderator",
                    LastName = "Moderator",
                    PhoneNumber = "",
                    EmailConfirmed = true
                };

                //Use UserManager to Create user defined by 
                await _userManager.CreateAsync(modUser, "YourPassword");

                //Add Admin Role to new User
                await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());
            }
        }
    }
}
