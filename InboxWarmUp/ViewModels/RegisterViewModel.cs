using System.ComponentModel.DataAnnotations;
namespace InboxWarmUp.ViewModels
{
    public class RegisterViewModel

    {
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Company Size is required.")]
        public string CompanySize { get; set; }

        public string CompanyWebsite { get; set; }

        public string CompanyType { get; set; }
    }
}
