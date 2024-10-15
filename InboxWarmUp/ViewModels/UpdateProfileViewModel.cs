using System.ComponentModel.DataAnnotations;

public class UpdateProfileViewModel
{
    public string UserId { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    // Add any other properties you want to update.
}
