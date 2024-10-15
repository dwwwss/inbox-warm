using InboxWarmUp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http; // Add this for accessing HttpContext

public class RecipientsController : Controller
{
    private readonly EmailDbContext _dbContext;

    public RecipientsController(EmailDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: Recipients/Index
    public IActionResult Index()
    {
        // Retrieve the userId from session
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            var recipients = _dbContext.EmailRecipients
                .Where(r => r.UserId == userId.Value) // Filter by UserId
                .ToList();
            return View(recipients);
        }

        // Handle the case where userId is not found
        return View(new List<EmailRecipient>());
    }

    [HttpPost]
    public IActionResult AddRecipient(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                // Add new recipient with the associated UserId
                _dbContext.EmailRecipients.Add(new EmailRecipient { Email = email, IsActive = true, UserId = userId.Value });
                _dbContext.SaveChanges();
            }
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RemoveRecipient(int id)
    {
        var recipient = _dbContext.EmailRecipients.Find(id);
        if (recipient != null)
        {
            _dbContext.EmailRecipients.Remove(recipient);
            _dbContext.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
