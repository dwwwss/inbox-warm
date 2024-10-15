using InboxWarmUp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace InboxWarmUp.Controllers
{
    public class OverviewController : Controller
    {
        private readonly EmailDbContext _context;

        public OverviewController(EmailDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalMailboxes = _context.EmailAccounts.Count();
            var activeMailboxes = _context.EmailAccounts.Count(e => e.IsActive);
            var inactiveMailboxes = totalMailboxes - activeMailboxes;
            var totalUsers = _context.Users.Count(); // Assuming you have a User DbSet
            var activeSessions = HttpContext.Session.Keys.Count(); // Example of getting active sessions
           /* var deletedMailboxes = _context.EmailAccounts.Count(e => e.IsDeleted); */// Assuming IsDeleted exists

            var model = new OverviewModel
            {
                TotalMailboxes = totalMailboxes,
                ActiveMailboxes = activeMailboxes,
                InactiveMailboxes = inactiveMailboxes,
                TotalUsers = totalUsers,
                ActiveSessions = activeSessions,
               
            };

            return View("~/Views/Email/Overview.cshtml", model);
        }
    }
}

