using Microsoft.AspNetCore.Mvc;
using InboxWarmUp.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

public class ScheduleController : Controller
{
    private readonly EmailDbContext _dbContext;

    public ScheduleController(EmailDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            var schedules = _dbContext.EmailSchedules
                .Where(s => s.UserId == userId.Value)
                .ToList();

            var emailAccounts = _dbContext.EmailAccounts
                .Where(ea => ea.UserId == userId.Value)
                .ToList();

            ViewBag.EmailAccounts = emailAccounts;

            return View(schedules);
        }
        else
        {
            ModelState.AddModelError("", "User is not logged in.");
            return View(new List<EmailSchedule>());
        }
    }

    [HttpPost]
    public IActionResult CreateSchedule(EmailSchedule schedule)
    {
        if (!ModelState.IsValid)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var emailAccounts = _dbContext.EmailAccounts
                    .Where(ea => ea.UserId == userId.Value)
                    .ToList();
                ViewBag.EmailAccounts = emailAccounts;
            }
            return View(schedule);
        }

        var loggedInUserId = HttpContext.Session.GetInt32("UserId");
        if (loggedInUserId.HasValue)
        {
            schedule.UserId = loggedInUserId.Value;
            _dbContext.EmailSchedules.Add(schedule);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "User is not logged in.");
        return View(schedule);
    }

    // Stop schedule action
    [HttpPost]
    public JsonResult StopSchedule(int id)
    {
        var schedule = _dbContext.EmailSchedules.FirstOrDefault(s => s.Id == id);
        if (schedule != null)
        {
            schedule.IsActive = false; // Deactivate the schedule
            _dbContext.SaveChanges();
            return Json(new { success = true, message = "Schedule stopped successfully." });
        }
        return Json(new { success = false, message = "Schedule not found." });
    }

    // Run schedule action
    [HttpPost]
    public JsonResult RunSchedule(int id)
    {
        var schedule = _dbContext.EmailSchedules.FirstOrDefault(s => s.Id == id);
        if (schedule != null)
        {
            schedule.IsActive = true; // Activate the schedule
            _dbContext.SaveChanges();
            return Json(new { success = true, message = "Schedule is running now." });
        }
        return Json(new { success = false, message = "Schedule not found." });
    }





    // Delete schedule action
    [HttpPost]
    public JsonResult DeleteSchedule(int id)
    {
        var schedule = _dbContext.EmailSchedules.FirstOrDefault(s => s.Id == id);
        if (schedule != null)
        {
            _dbContext.EmailSchedules.Remove(schedule); // Delete the schedule
            _dbContext.SaveChanges();
            return Json(new { success = true, message = "Schedule deleted successfully." });
        }
        return Json(new { success = false, message = "Schedule not found." });
    }

}
