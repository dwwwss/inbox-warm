namespace InboxWarmUp.Models
{
    public class EmailRecipient
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }  // To mark if the email is active for sending

        public int UserId { get; set; }
        public User User { get; set; } // Navigation property for the associated User
    }
}
