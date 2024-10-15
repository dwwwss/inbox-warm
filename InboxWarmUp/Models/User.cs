namespace InboxWarmUp.Models
{
    public class User
    {
        public int Id { get; set; } // Primary key for the User
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Additional properties for company details (can be null)
        public string? CompanyName { get; set; } // Name of the company (nullable)
        public string? CompanySize { get; set; } // Size of the company (e.g., Small, Medium, Large) (nullable)
        public string? CompanyWebsite { get; set; } // Website of the company (nullable)
        public string? CompanyType { get; set; } // Type of company (e.g., LLC, Corporation, etc.) (nullable)

     /*   public string ResetToken { get; set; } // Add this property*/
        // Navigation properties
        public ICollection<EmailAccount> EmailAccounts { get; set; }
        public ICollection<EmailRecipient> EmailRecipients { get; set; }
        public ICollection<EmailSchedule> EmailSchedules { get; set; }
    }
}
