namespace InboxWarmUp.Models
{
    namespace InboxWarmUp.ViewModels
    {
        public class RegisterViewModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }

            // Additional properties for company details (can be null)
            public string? CompanyName { get; set; } // Name of the company (nullable)
            public string? CompanySize { get; set; } // Size of the company (e.g., Small, Medium, Large) (nullable)
            public string? CompanyWebsite { get; set; } // Website of the company (nullable)
            public string? CompanyType { get; set; } // Type of company (e.g., LLC, Corporation, etc.) (nullable)

        }

    }

}
