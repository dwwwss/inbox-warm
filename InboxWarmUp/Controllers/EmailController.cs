using Microsoft.AspNetCore.Mvc;
using InboxWarmUp.Models; // Ensure this namespace is included
using System.Linq;
using System.Security.Claims; // For ClaimTypes
using Microsoft.EntityFrameworkCore; // If you need it for DbContext

namespace InboxWarmUp.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailDbContext _dbContext;

        public EmailController(EmailDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Display the creation form
        public IActionResult Create()
        {
            return View(); // Return the view for creating an email account
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmailAccount emailAccount)
        {
            // Retrieve userId from the session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                // Set the UserId dynamically from session
                emailAccount.UserId = userId.Value; // Set the UserId from session
                emailAccount.IsActive = true; // Assuming accounts are active by default

                // Add the new EmailAccount to the DbContext
                _dbContext.EmailAccounts.Add(emailAccount);
                await _dbContext.SaveChangesAsync(); // Ensure you await the async call

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Invalid User ID.");
            return View(emailAccount); // Return the same view with the current model
        }


        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                var emailAccounts = _dbContext.EmailAccounts
                                               .Where(e => e.UserId == userId.Value)
                                               .ToList();

                return View(emailAccounts);
            }

            // Handle the case where UserId is not found
            return View(new List<EmailAccount>());
        }


        [HttpPost]
        public IActionResult RemoveEmailAccount(int id)
        {
            var emailAccount = _dbContext.EmailAccounts.Find(id);
            if (emailAccount != null)
            {
                // Find and remove related EmailSchedules first
                var relatedEmailSchedules = _dbContext.EmailSchedules
                    .Where(e => e.EmailAccountId == id)
                    .ToList();

                if (relatedEmailSchedules.Any())
                {
                    _dbContext.EmailSchedules.RemoveRange(relatedEmailSchedules);
                }

                _dbContext.EmailAccounts.Remove(emailAccount);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RunEmailAccount(int id)
        {
            var emailAccount = _dbContext.EmailAccounts.Find(id);
            if (emailAccount != null)
            {
                // Logic to run the email account
                emailAccount.IsActive = true; // Set IsActive to true

                // Optionally, you might want to add logic to start any associated processes/tasks
                // For example, you might want to add code to start email sending/processing

                // Save changes to the database
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult StopRun(int id)
        {
            var emailAccount = _dbContext.EmailAccounts.Find(id);
            if (emailAccount != null)
            {
                emailAccount.IsActive = false; // Set IsActive to false

                // Optionally remove associated schedules if needed
                var relatedEmailSchedules = _dbContext.EmailSchedules
                    .Where(e => e.EmailAccountId == id)
                    .ToList();

                if (relatedEmailSchedules.Any())
                {
                    _dbContext.EmailSchedules.RemoveRange(relatedEmailSchedules);
                }

                // Save changes to the database
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }


}

