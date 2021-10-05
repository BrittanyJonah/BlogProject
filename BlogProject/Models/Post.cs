using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PersonalBlog.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalBlog.Models
{
    /// <summary>
    /// Creates a single post within a blog category. Has an author and can have comments.
    /// </summary>
    public class Post
    {
        //Keys
        public int Id { get; set; } //PK for the Post

        [Display(Name = "Blog Name")]
        public int BlogId { get; set; } //FK linking to PK in Blog class

        public string BlogUserId { get; set; } //FK - PK BlogUserId in Users table

        //Descriptive Properties
        [Required]
        [StringLength(75, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        public string Abstract { get; set; } //Similar to desription

        [Required]
        public string Content { get; set; }

        //Time
        [DataType(DataType.Date)]
        [Display(Name = "Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Updated")]
        public DateTime? Updated { get; set; } //nullable

        public PostLocation PostLocation { get; set; }//determines if Post is a main highlight

        public string Slug { get; set; } //Used for SEO/Custom Routing

        public int PageViews { get; set; } //tracks number of viewers

        //Image
        [Display(Name = "Blog Post Image")]
        public byte[] ImageData { get; set; }
        [Display(Name = "Image Type")]
        public string ContentType { get; set; }
        [NotMapped]  //Excludes this from database mapping
        public IFormFile Image { get; set; }

        //Navigation Properties
        public virtual Blog Blog { get; set; } //parent
        public virtual BlogUser BlogUser { get; set; } //parent

        //Children
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
