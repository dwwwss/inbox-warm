using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InboxWarmUp.Models
{
    public class EmailSchedule
    {
        public int Id { get; set; }
        public string Time { get; set; }
        public string Days { get; set; }
        public int Frequency { get; set; }
        public bool IsActive { get; set; }

        public int UserId { get; set; }  // Foreign key for the User
        public int? EmailAccountId { get; set; }  // Make this nullable

        [ValidateNever] // Tell model binder to ignore this during validation
        public User? User { get; set; }

        // Navigation property to EmailAccount
        public EmailAccount? EmailAccount { get; set; }  // Make this nullable as well
    }

}
