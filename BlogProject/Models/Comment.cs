using Microsoft.AspNetCore.Identity;
using PersonalBlog.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PersonalBlog.Models
{
    /// <summary>
    /// Creates a single Comment. Comments are left on the bottom of Posts. 
    /// Can be created by Users and Edited by Moderators.
    /// </summary>
    public class Comment
    {
        //Keys
        public int Id { get; set; } //PK For comment
        public int PostId { get; set; } //FK, Links to Post PK
        public string BlogUserId { get; set; } //FK - PK BlogUserId in Users table
        public string ModeratorId { get; set; } //FK - PK ModeratorId in Users Table

        //Description Properties
        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be between {2} and {1} characters long", MinimumLength = 2)]
        [Display(Name = "Comment")]
        public string Body { get; set; } //The body of the Comment

        //Time
        public DateTime Created { get; set; }
        public DateTime? Update { get; set; } //nullable
        public DateTime? Moderated { get; set; } //nullable
        public DateTime? Deleted { get; set; } //nullable

        /// <summary>
        /// Display Moderated body instead of Body when Comment has been edited by Moderator
        /// </summary>
        [StringLength(500, ErrorMessage = "The {0} must be between {2} and {1} characters long", MinimumLength = 2)]
        [Display(Name = "Moderated Comment")]
        public string ModeratedBody { get; set; }

        public ModerationType ModerationType { get; set; } //Forces Moderator to choose reason for moderation from a list

        //Navigation Properties
        public virtual Post Post { get; set; } //PostId Without the Id as naming convention.
                                               //Holds entire record that is represented within single Property of FK
        public virtual BlogUser BlogUser { get; set; } //Default type is BlogUser
        public virtual BlogUser Moderator { get; set; }

    }
}
