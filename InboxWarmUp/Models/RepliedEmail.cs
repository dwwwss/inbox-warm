using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InboxWarmUp.Models
{
    public class RepliedEmail
    {
        [Key]
        public int Id { get; set; } // Primary key for the replied email

        [ForeignKey("EmailAccount")]
        public int EmailAccountId { get; set; } // Foreign key to the EmailAccount table
        public EmailAccount EmailAccount { get; set; } // Navigation property to the associated EmailAccount

        [Required] // Ensure this field is required and not null
        public string OriginalEmailId { get; set; } // ID of the original email to which this is a reply
        public string ReplyContent { get; set; } // The content of the reply email
        public DateTime ReplyDate { get; set; } // Date and time when the reply was sent

        public string RecipientEmail { get; set; } // Email of the recipient to whom the reply was sent
        public string Subject { get; set; } // Subject of the reply email

        // Optional: Add other fields as necessary
    }
}
