/*using InboxWarmUp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class UserController : Controller
{
    private readonly EmailDbContext _dbContext;

    public UserController(EmailDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Fetch user profile by userId
    [HttpGet("User/Profile/{userId}")]
    public IActionResult Profile(int userId)
    {
        var user = _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.FirstName,
                u.LastName,
                u.Email,
                u.Password
            })
            .FirstOrDefault();

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var model = new User
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
        };

        return View("~/Views/Email/Profile.cshtml", model);
    }


    private int GetLoggedInUserId()
    {
        // Ensure the user is authenticated
        if (User.Identity.IsAuthenticated)
        {
            // Get the user ID from the claims
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        throw new UnauthorizedAccessException("User is not authenticated.");
    }

}
*/