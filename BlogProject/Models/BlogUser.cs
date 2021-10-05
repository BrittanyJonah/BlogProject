using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalBlog.Models
{
    public class BlogUser : IdentityUser
    {
        //Name
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName {
            get {
                return $"{FirstName} {LastName}";
            }
        }

        //Image
        [Display(Name = "Image")]
        public byte[] ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string ContentType { get; set; }

        //Links
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }

        //Navigation Properties
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>(); //blogs created by User
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>(); //posts written by User

    }
}
