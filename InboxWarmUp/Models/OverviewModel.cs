namespace InboxWarmUp.Models
{
    public class OverviewModel
    {
        // Existing properties
        public int TotalMailboxes { get; set; }
        public int ActiveMailboxes { get; set; }
        public int InactiveMailboxes { get; set; }
        public int TotalUsers { get; set; }
        // New properties for email account creation
        public int ActiveSessions { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}
