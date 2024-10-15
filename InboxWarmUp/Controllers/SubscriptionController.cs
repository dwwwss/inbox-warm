using InboxWarmUp.Models; // Ensure this matches your UserProfile namespace
using Microsoft.AspNetCore.Mvc;

namespace InboxWarmUp.Controllers // Ensure this matches your actual namespace
{
    public class SubscriptionController : Controller
    {
        private readonly EmailDbContext _dbContext;

        public SubscriptionController(EmailDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /Subscription
        public IActionResult Index()
        {
            var model = new Subscription(); // Initialize model
            return View("~/Views/Purchase/Subscription.cshtml", model); // Pass the model to the view
        }

        // POST: /Subscription/Update
        [HttpPost]
        public IActionResult Update(Subscription model)
        {
            if (ModelState.IsValid) // Check if the model state is valid
            {
                // Logic to update the subscription based on selectedAmountInput
                // This could involve updating a database, etc.

                // Calculate the new price based on the selected amount
                model.CalculatedPrice = model.SelectedAmount * 10; // Adjust this as needed

                // You can save the updated subscription to the database if necessary
                // _dbContext.Subscriptions.Update(model);
                // _dbContext.SaveChanges();

                // Pass the updated model back to the view
                return View("~/Views/Purchase/Subscription.cshtml", model);
            }

            // If the model state is not valid, return the same view with the model
            return View("~/Views/Purchase/Subscription.cshtml", model);
        }
    }
}
