using Microsoft.EntityFrameworkCore;

namespace InboxWarmUp.Models
{
    public class EmailDbContext : DbContext
    {
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailRecipient> EmailRecipients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RepliedEmail> RepliedEmails { get; set;}
        public DbSet<EmailSchedule> EmailSchedules { get; set; }

        public EmailDbContext(DbContextOptions<EmailDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the relationships for the User entity
            modelBuilder.Entity<User>()
                .HasMany(u => u.EmailAccounts)
                .WithOne(ea => ea.User)
                .HasForeignKey(ea => ea.UserId) // Ensure EmailAccount has a UserId property
                .OnDelete(DeleteBehavior.Cascade); // Set cascade delete if desired

            modelBuilder.Entity<User>()
                .HasMany(u => u.EmailRecipients)
                .WithOne(er => er.User)
                .HasForeignKey(er => er.UserId) // Ensure EmailRecipient has a UserId property
                .OnDelete(DeleteBehavior.Cascade); // Set cascade delete if desired

            modelBuilder.Entity<User>()
                .HasMany(u => u.EmailSchedules)
                .WithOne(es => es.User)
                .HasForeignKey(es => es.UserId) // Ensure EmailSchedule has a UserId property
                .OnDelete(DeleteBehavior.Cascade); // Set cascade delete if desired

            // Update the relationship for EmailSchedules to EmailAccount to avoid cascade issues
            modelBuilder.Entity<EmailSchedule>()
                .HasOne(es => es.EmailAccount)
                .WithMany(ea => ea.EmailSchedules)
                .HasForeignKey(es => es.EmailAccountId) // Ensure EmailSchedule has an EmailAccountId property
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of EmailAccount if there are EmailSchedules referencing it

            // Configure properties for User entity
            modelBuilder.Entity<User>()
                .Property(u => u.CompanyName)
                .HasMaxLength(100) // Set a maximum length for the Company Name
                .IsRequired(false); // Optional: You can specify if it's required

            modelBuilder.Entity<User>()
                .Property(u => u.CompanySize)
                .HasMaxLength(50) // Set a maximum length for the Company Size
                .IsRequired(false); // Optional: You can specify if it's required

            modelBuilder.Entity<User>()
                .Property(u => u.CompanyWebsite)
                .HasMaxLength(200) // Set a maximum length for the Company Website
                .IsRequired(false); // Optional: You can specify if it's required

            modelBuilder.Entity<User>()
                .Property(u => u.CompanyType)
                .HasMaxLength(50) // Set a maximum length for the Company Type
                .IsRequired(false); // Optional: You can specify if it's required
        }

    }
}
