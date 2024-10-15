using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;
public class EmailService
{
    public async Task SendEmailAsync(string senderEmail, string senderPassword, string senderName, string recipientEmail, string subject, string body, string smtpProvider)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail)); // Use dynamic sender
        message.To.Add(new MailboxAddress("", recipientEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();
        using (var client = new SmtpClient()) // Use MailKit's SmtpClient
        {
            try
            {
                // Choose SMTP server based on the provider (Hostinger or Gmail)
                switch (smtpProvider.ToLower())
                {
                    case "hostinger":
                        client.Connect("smtp.hostinger.com", 465, true); // Hostinger SMTP server (SSL)
                        break;
                    case "gmail":
                        client.Connect("smtp.gmail.com", 465, true); // Gmail SMTP server (SSL)
                        break;
                    default:
                        throw new Exception("Unsupported SMTP provider");
                }
                client.Authenticate(senderEmail, senderPassword); // Authenticate with the sender's email and password
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Handle SMTP exceptions here (log or retry logic)
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
            finally
            {
                client.Disconnect(true);
            }
        }
    }
}