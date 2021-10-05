using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PersonalBlog.Models
{
    /// <summary>
    /// Tags are used for grouping or aggregating Posts. They give more details about the Post.
    /// Tags can be Added/Removed from Posts
    /// </summary>
    public class Tag
    {
        //Keys
        public int Id { get; set; } // PK for Tag
        public int PostId { get; set; } //FK, Links to Post PK
        public string BlogUserId { get; set; }  //FK - PK BlogUserId in Users table

        //Description Properties
        [Required]
        [StringLength(25, ErrorMessage = "The {0} must be between {2} and {25} characters long.", MinimumLength = 2)]
        public string Text { get; set; } //Text of the Tag
        
        //Navigation Properties
        public virtual Post Post { get; set; } //entire record within PostId 
        public virtual BlogUser BlogUser { get; set; }
    }
}
