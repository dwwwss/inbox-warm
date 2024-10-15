using InboxWarmUp.Models;
using InboxWarmUp.Services;
using MailKit.Search;
using MailKit;
using MimeKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using System.Linq;
public class EmailReplyService
{
    private readonly EmailDbContext _dbContext;
    private readonly AIService _aiService;
    private readonly EmailService _emailService;
    public EmailReplyService(EmailDbContext dbContext, AIService aiService, EmailService emailService)
    {
        _dbContext = dbContext;
        _aiService = aiService;
        _emailService = emailService;
    }
    public async Task CheckEmailInboxAsync(EmailAccount emailAccount)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(emailAccount.ImapHost, 993, true);
                client.Authenticate(emailAccount.Email, emailAccount.Password);
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);
                // Fetch all active recipients from the database
                var activeRecipients = await _dbContext.EmailRecipients
                    .Where(er => er.IsActive)
                    .Select(er => er.Email.ToLower())
                    .ToListAsync();
                if (!activeRecipients.Any())
                {
                    Console.WriteLine("No active recipients found.");
                    return;
                }
                // Fetch all emails in the inbox
                var query = SearchQuery.All;
                var uids = await inbox.SearchAsync(query);
                // Check if there are any messages
                if (!uids.Any())
                {
                    Console.WriteLine("No messages found.");
                    return;
                }
                // Process the latest email for each active recipient
                foreach (var recipient in activeRecipients)
                {
                    // Fetch the latest email from this recipient
                    var recipientEmails = await inbox.SearchAsync(
                        SearchQuery.FromContains(recipient)
                    );
                    if (recipientEmails.Any())
                    {
                        var latestRecipientEmailUid = recipientEmails.Last(); // Get the latest email UID for the recipient
                        var message = await inbox.GetMessageAsync(latestRecipientEmailUid);
                        var senderEmail = message.From.Mailboxes.First().Address.ToLower();
                        if (message.InReplyTo != null)
                        {
                            Console.WriteLine($"Processing reply email from {senderEmail} with subject {message.Subject}");
                            await ProcessEmailMessage(message, emailAccount);
                        }
                        else
                        {
                            Console.WriteLine($"Ignoring non-reply email from {senderEmail} with subject {message.Subject}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No recent emails found from {recipient}");
                    }
                }
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking email inbox: {ex.Message}");
        }
    }
    private async Task ProcessEmailMessage(MimeMessage message, EmailAccount emailAccount)
    {
        var senderEmail = message.From.Mailboxes.First().Address;
        var body = message.TextBody;
        // Log the received message
        Console.WriteLine($"Received message from: {senderEmail}, Body: {body}");
        // Check if a reply has already been sent for this message
        var hasReplied = await CheckReplyStatus(senderEmail, message.Subject);
        if (hasReplied)
        {
            Console.WriteLine($"Reply already sent to {senderEmail} for subject: {message.Subject}. Skipping reply.");
            return;
        }
        // Generate an AI response based on the body of the reply
        string replyContent = await GenerateAIResponse(senderEmail, body, emailAccount);
        // Log the AI response
        Console.WriteLine($"Generated reply for {senderEmail}: {replyContent}");
        // Sending the email back to the sender
        try
        {
            await _emailService.SendEmailAsync(
                emailAccount.Email,
                emailAccount.Password,
                emailAccount.DisplayName,
                senderEmail,
                "Re: " + message.Subject,
                replyContent,
                emailAccount.SmtpProvider
            );
            Console.WriteLine("Reply sent successfully.");
            string originalEmailId = message.MessageId; // Use the MessageId or another identifier
            // Mark that a reply has been sent
            await MarkAsReplied(senderEmail, message.Subject, originalEmailId, replyContent, senderEmail, emailAccount.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending reply to {senderEmail}: {ex.Message}");
        }
    }
    public async Task<List<EmailAccount>> GetEmailAccounts()
    {
        // Fetch all active email accounts from the database
        return await _dbContext.EmailAccounts
            .Where(account => account.IsActive) // Filter only active accounts
            .ToListAsync();
    }
    private async Task<string> GenerateAIResponse(string recipientEmail, string receivedMessage, EmailAccount emailAccount)
    {
        // Determine time-based greeting
        string greeting;
        var currentHour = DateTime.Now.Hour;
        if (currentHour < 12)
            greeting = "Good morning";
        else if (currentHour < 18)
            greeting = "Good afternoon";
        else
            greeting = "Good evening";
        // Use the sender's display name for sign-off
        string senderName = emailAccount.DisplayName;
        // Customize the prompt to improve reply quality
        string prompt = $@"
        {greeting}, {recipientEmail.Split('@')[0]}.
        You received the following message from {recipientEmail}:
        {receivedMessage}
        Please draft a professional and relevant reply with a human-like tone. The response should feel natural, address any queries or concerns from the original message, and offer assistance or next steps if needed. Maintain a friendly yet formal tone, avoiding overly technical jargon unless required.
        Close the message with 'Best regards, {senderName}'.";
        // Generate the response from AI service
        var aiResponse = await _aiService.GenerateEmailTemplateAsync(prompt);
        // Add a fallback default reply if the AI response is empty
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            aiResponse = $"{greeting},\n\nThank you for your email. I will review it and get back to you shortly.\n\nBest regards,\n{senderName}";
        }
        // Ensure the AI-generated response includes a personalized sign-off with the sender's name
        if (!aiResponse.Contains("Best regards"))
        {
            aiResponse += $"\n\nBest regards,\n{senderName}";
        }
        return aiResponse;
    }
    private async Task<bool> CheckReplyStatus(string senderEmail, string subject)
    {
        // Check in the database if a reply has been sent for this email
        var replyExists = await _dbContext.RepliedEmails
            .AnyAsync(re => re.RecipientEmail == senderEmail && re.Subject == subject);
        return replyExists;
    }
    private async Task MarkAsReplied(string senderEmail, string subject, string originalEmailId, string replyContent, string recipientEmail, int emailAccountId)
    {
        // Ensure required values are not null or empty
        if (string.IsNullOrEmpty(originalEmailId))
        {
            throw new ArgumentException("OriginalEmailId cannot be null or empty.", nameof(originalEmailId));
        }
        if (string.IsNullOrEmpty(replyContent))
        {
            throw new ArgumentException("ReplyContent cannot be null or empty.", nameof(replyContent));
        }
        if (string.IsNullOrEmpty(recipientEmail))
        {
            throw new ArgumentException("RecipientEmail cannot be null or empty.", nameof(recipientEmail));
        }
        // Create a new replied email instance
        var repliedEmail = new RepliedEmail
        {
            EmailAccountId = emailAccountId, // Set the ID of the email account
            OriginalEmailId = originalEmailId, // Set the ID of the original email
            ReplyContent = replyContent, // Set the content of the reply
            ReplyDate = DateTime.Now, // Set the date and time of the reply
            RecipientEmail = recipientEmail, // Set the recipient's email
            Subject = subject // Set the subject of the reply
        };
        // Add the replied email to the context
        _dbContext.RepliedEmails.Add(repliedEmail);
        // Save changes asynchronously
        await _dbContext.SaveChangesAsync();
    }
}