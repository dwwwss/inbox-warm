using Microsoft.AspNetCore.Identity;

namespace InboxWarmUp.Models
{
    public class EmailAccount
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string SmtpProvider { get; set; } // SMTP provider (e.g., Gmail)
        public string ImapHost { get; set; } // IMAP host for reading emails
        public int ImapPort { get; set; } // Port for IMAP, typically 993 for SSL
        public bool IsActive { get; set; } // To manage active accounts
        public ICollection<EmailSchedule> EmailSchedules { get; set; }

        // Foreign key for the User
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property for the associated User
    }
}
