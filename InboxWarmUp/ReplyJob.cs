using Quartz;
public class ReplyJob : IJob
{
    private readonly EmailReplyService _emailReplyService;
    public ReplyJob(EmailReplyService emailReplyService)
    {
        _emailReplyService = emailReplyService;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        // Fetch all active email accounts from your database and check for replies
        var emailAccounts = await _emailReplyService.GetEmailAccounts();
        foreach (var account in emailAccounts)
        {
            Console.WriteLine($"Checking email for account: {account.Email}, IMAP Host: {account.ImapHost}");
            await _emailReplyService.CheckEmailInboxAsync(account);
        }
        Console.WriteLine("Checked email replies.");
    }
}
