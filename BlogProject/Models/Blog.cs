using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalBlog.Models
{
    /// <summary>
    /// A single instance of a Blog which will have a description, image, Author and contain Posts
    /// </summary>
    public class Blog
    {
        //Keys
        public int Id { get; set; } //PK for blog
        public string BlogUserId { get; set; } //FK - PK BlogUserId in Users table

        //Descriptive Properties
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string Description { get; set; }

        //Dates
        [DataType(DataType.Date)] //Treat as Date only when picking DateTime in UI
        [Display(Name = "Date Created")] //Displayed in a label by default
        public DateTime Created { get; set; } //default is not nullable
        
        [DataType(DataType.Date)] 
        [Display(Name = "Date Updated")] 
        public DateTime? Updated { get; set; } //nullable

        public string Slug { get; set; } //Used for SEO/Custom Routing

        //Images
        [Display(Name = "Image")]
        public byte[] ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string ContentType { get; set; }

        [NotMapped] //Excludes this from database mapping
        public IFormFile Image { get; set; }

        //Navigation Properties
        [Display(Name = "Author")]
        public virtual BlogUser BlogUser { get; set; } //Navigate from a Blog to its Author by using the FK.
                                                     //Making it a "parent" to Blog
        //Children
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>(); //HashSet is good to deal with with FKs

    }
}
