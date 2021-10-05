using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalBlog.Data;
using PersonalBlog.Models;
using PersonalBlog.Services;
using PersonalBlog.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Connection String - driver determines which backend 
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql( //driver
                    ConnectionService.GetConnectionString(Configuration))); //goes to appsettings.json file to get DefaultConnection

            services.AddDatabaseDeveloperPageExceptionFilter();

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            //Updated to register service to BlogUser
            services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
              //Services Injected
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();

            //Added Razorpages
            services.AddRazorPages();

            //Register Custom DataService class
            services.AddScoped<DataService>();
            // Register Search Service
            services.AddScoped<BlogSearchService>();

            //Register a pre configured instance of MailSettings class
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            services.AddScoped<IBlogEmailSender, EmailService>();

            //Register Image Service
            services.AddScoped<IImageService, BasicImageService>();

            //Register Slug Service
            services.AddScoped<ISlugService, BasicSlugService>();

            //Modify the default settings for password requirements
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
            });
        }

        private Action<object> MailSettings()
        {
            throw new NotImplementedException();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "test",
                    pattern: "Categories/",
                    defaults: new { controller = "Blogs", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "SlugRouteBlog",
                    pattern: "Categories/{slug}",
                    defaults: new { controller = "Blogs", action = "Details" });

                endpoints.MapControllerRoute(
                    name: "SlugRoutePost",
                    pattern: "Categories/{blog}/{slug}",
                    defaults: new { controller = "Posts", action = "Details" });


                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
