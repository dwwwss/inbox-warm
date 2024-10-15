namespace InboxWarmUp.Models
{
    public class Subscription
    {
        public int SelectedAmount { get; set; } // Number of mailboxes selected
        public decimal CalculatedPrice { get; set; } // Total calculated price
    }
}
