using InboxWarmUp.Models;
using InboxWarmUp.Services;
using Quartz;
using System;
using System.Collections.Generic; // Required for List
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
public class EmailJob : IJob
{
    private readonly ILogger<EmailJob> _logger;
    private readonly EmailService _emailService;
    private readonly EmailDbContext _dbContext;
    private readonly AIService _aiService;
    private readonly EmailReplyService _emailReplyService;
    public EmailJob(ILogger<EmailJob> logger, EmailService emailService, EmailDbContext dbContext, AIService aiService, EmailReplyService emailReplyService)
    {
        _logger = logger;
        _emailService = emailService;
        _dbContext = dbContext;
        _aiService = aiService;
        _emailReplyService = emailReplyService;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Email job started at {time}", DateTime.UtcNow);
        // Retrieve active email schedules
        var schedules = _dbContext.EmailSchedules.Where(s => s.IsActive).ToList();
        List<int> selectedRecipientIds = new List<int>();
        foreach (var schedule in schedules)
        {
            try
            {
                // Only proceed if today is the correct day and the time is correct
                if (IsScheduledDay(schedule.Days) && IsScheduledTime(schedule.Time))
                {
                    var selectedAccount = _dbContext.EmailAccounts.FirstOrDefault(e => e.Id == schedule.EmailAccountId && e.IsActive);
                    if (selectedAccount == null)
                    {
                        _logger.LogWarning("No active email account found for schedule ID {scheduleId}.", schedule.Id);
                        continue;
                    }
                    // Get all active recipients excluding already selected ones
                    var allActiveRecipients = _dbContext.EmailRecipients
                        .Where(e => e.IsActive && !selectedRecipientIds.Contains(e.Id))
                        .ToList();
                    if (!allActiveRecipients.Any())
                    {
                        _logger.LogWarning("No available active recipients for schedule ID {scheduleId}.", schedule.Id);
                        continue;
                    }
                    // Validate the frequency (recipient count)
                    int recipientCount = schedule.Frequency > 0 ? schedule.Frequency : 5; // Default to 5 if not set or invalid
                    var randomRecipients = allActiveRecipients
                        .OrderBy(r => Guid.NewGuid()) // Randomize the order
                        .Take(recipientCount) // Select number of recipients as per the frequency
                        .ToList();
                    foreach (var recipient in randomRecipients)
                    {
                        try
                        {
                            // Fetch user-specific details
                            var userDetails = await GetUserDetails(recipient.UserId); // Fetch company name and product details
                            string senderCompany = userDetails.SenderCompany; // User's company
                            string productDetails = userDetails.ProductDetails; // User's product details
                            var subject = await GenerateEmailSubject(recipient.Email, senderCompany, productDetails);
                            var emailBody = await GenerateEmailBody(recipient.Email, selectedAccount.DisplayName, senderCompany, productDetails);
                            _logger.LogInformation("Generated email for {email}: {emailBody}", recipient.Email, emailBody);
                            await _emailService.SendEmailAsync(
                                selectedAccount.Email,
                                selectedAccount.Password,
                                selectedAccount.DisplayName,
                                recipient.Email,
                                subject,
                                emailBody,
                                selectedAccount.SmtpProvider
                            );
                            LogSentEmail(recipient.Email, subject, emailBody);
                            selectedRecipientIds.Add(recipient.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send email to {email}", recipient.Email);
                        }
                    }
                    // Check for replies after sending emails
                    await _emailReplyService.CheckEmailInboxAsync(selectedAccount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in processing schedule ID {scheduleId}", schedule.Id);
            }
        }
        _logger.LogInformation("Email job finished at {time}", DateTime.UtcNow);
    }
    private async Task<(string SenderCompany, string ProductDetails)> GetUserDetails(int userId)
    {
        // Fetch user details based on userId
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        // Assuming the User model has these properties
        string senderCompany = user.CompanyName; // Adjust the property name according to your model
        string productDetails = "lms,hrms"; // Adjust the property name according to your model
        return (senderCompany, productDetails);
    }
    /*  private async Task<string> GenerateEmailBody(string recipientEmail, string senderName, string senderCompany, string productDetails)
      {
          // Create a personalized prompt for AI
          string prompt = GenerateDynamicMarketingPrompt(recipientEmail, senderName, senderCompany, productDetails);
          var emailBody = await _aiService.GenerateEmailTemplateAsync(prompt);
          // Check for empty response and provide a structured fallback
          if (string.IsNullOrWhiteSpace(emailBody))
          {
              emailBody = $@"
  Hi {recipientEmail},
  I hope this message finds you well!
  I wanted to take a moment to introduce you to our latest product, {productDetails}.
  At {senderCompany}, we believe this could significantly benefit your operations by [insert specific benefits or features here].
  If you're interested, I would love to schedule a time to discuss this further.
  Looking forward to hearing from you!
  Best wishes,
  {senderName}";
          }
          else
          {
              if (!emailBody.Contains("Best regards"))
              {
                  emailBody += $"\n\nBest regards,\n{senderName}";
              }
          }
          return emailBody;
      }*/

    private async Task<string> GenerateEmailBody(string recipientEmail, string senderName, string senderCompany, string productDetails)
    {
        // Create a personalized prompt for AI
        string prompt = GenerateDynamicMarketingPrompt(recipientEmail, senderName, senderCompany, productDetails);
        var emailBody = await _aiService.GenerateEmailTemplateAsync(prompt);

      /*  // Check for empty response and provide a structured fallback
        if (string.IsNullOrWhiteSpace(emailBody))
        {
            // Fallback structure with clear sections, warm greeting, and regards at the end
            emailBody = $@"
<p>Dear {recipientEmail},</p>
<br>
<p>I hope this message finds you well!</p>
<p>I am excited to introduce our latest product from <b>{senderCompany}</b>: <br>{productDetails}.</p>
<p>We believe this could significantly benefit your operations, and I would love to schedule a time to discuss this further. Please don't hesitate to reach out for more details.</p>
<p>Looking forward to hearing from you soon!</p>
<p><br>Best regards,<br>{senderName}<br>{senderCompany}</p>";
        }
        else
        {
            // Ensure "Best regards" is only at the end
            int firstOccurrenceIndex = emailBody.IndexOf("Best regards");

            // If "Best regards" appears earlier, remove it
            if (firstOccurrenceIndex != -1 && firstOccurrenceIndex != emailBody.LastIndexOf("Best regards"))
            {
                emailBody = emailBody.Remove(firstOccurrenceIndex, emailBody.IndexOf("</p>", firstOccurrenceIndex) + 4 - firstOccurrenceIndex);
            }

            // Ensure "Best regards" is added at the very end
            if (!emailBody.Trim().EndsWith("Best regards"))
            {
                emailBody += $@"<br><p>Best regards,<br>{senderName}<br>{senderCompany}</p>";
            }
        }
*/
        return emailBody;
    }

    private async Task<string> GenerateEmailSubject(string recipientEmail, string senderCompany, string productDetails)
    {
        // Define a clear and concise AI prompt for generating the subject based on the content
        string prompt = $"Generate a concise and catchy 4-word subject related to {productDetails} from {senderCompany}.";

        var subject = await _aiService.GenerateEmailTemplateAsync(prompt);

        // Fallback subject if AI response is empty or not suitable
        if (string.IsNullOrWhiteSpace(subject) || subject.Split().Length > 4)
        {
            subject = $"{productDetails}: Improve with {senderCompany}"; // Short fallback subject
        }

        return subject;
    }

    private string GenerateDynamicMarketingPrompt(string recipientEmail, string senderName, string senderCompany, string productDetails)
    {
        return $@"Write a 4-line short professional marketing email to {recipientEmail} from {senderCompany}, introducing our product: {productDetails}.
Highlight the key benefits it offers and encourage them to respond for more information or a meeting.
Include a friendly greeting and end with the sender's name {senderName} in the closing.
Don't add a subject in the body; only start with greetings.";
    }


    private bool IsScheduledDay(string scheduledDays)
    {
        var today = DateTime.Now.DayOfWeek.ToString();
        return scheduledDays.Split(',').Contains(today, StringComparer.OrdinalIgnoreCase);
    }
    private bool IsScheduledTime(string scheduledTime)
    {
        var currentTime = DateTime.Now.ToString("HH:mm");
        return currentTime == scheduledTime;
    }
    private void LogSentEmail(string recipientEmail, string subject, string body)
    {
        // Implement your logging logic here (e.g., saving to the database)
    }
}