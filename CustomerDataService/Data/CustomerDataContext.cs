using CustomerDataDomainModels.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerDataService.Data
{
    /// <summary>
    /// Represents the database context for customer data, campaigns, users, and emails in the application. 
    /// Configures entity relationships and cascade delete behaviors within the database.
    /// </summary>
    public class CustomerDataContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDataContext"/> class with specified options.
        /// </summary>
        /// <param name="options">Options to configure the database context.</param>
        public CustomerDataContext(DbContextOptions<CustomerDataContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{Campaign}"/> representing the Campaign entities in the database.
        /// </summary>
        public DbSet<Campaign> Campaign { get; set; } = default!;

        /// <summary>
        /// Gets or sets the <see cref="DbSet{CustomerData}"/> representing the CustomerData entities in the database.
        /// </summary>
        public DbSet<CustomerData> CustomerData { get; set; } = default!;

        /// <summary>
        /// Gets or sets the <see cref="DbSet{SendsEmails}"/> representing the SendsEmails entities in the database.
        /// </summary>
        public DbSet<SendsEmails> SendsEmails { get; set; } = default!;

        /// <summary>
        /// Gets or sets the <see cref="DbSet{UserInfo}"/> representing the UserInfo entities in the database.
        /// </summary>
        public DbSet<UserInfo> UserInfo { get; set; } = default!;

        /// <summary>
        /// Configures entity relationships and cascade delete behaviors for the database.
        /// Defines relationships between Campaign, CustomerData, and SendsEmails entities.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the entity model for the database context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configures Campaign -> SendsEmails with Cascade Delete
            modelBuilder.Entity<Campaign>()
                .HasMany(cm => cm.EmailsSent)
                .WithOne(se => se.Campaign)
                .OnDelete(DeleteBehavior.Cascade);

            // Configures CustomerData -> SendsEmails with Cascade Delete
            modelBuilder.Entity<CustomerData>()
                .HasMany(cd => cd.EmailsSent)  // `cd` for clarity
                .WithOne(se => se.CustomerData)
                .OnDelete(DeleteBehavior.Cascade);

            // Defines the foreign key relationship between SendsEmails and Campaign with Cascade Delete
            modelBuilder.Entity<SendsEmails>()
                .HasOne(se => se.Campaign)
                .WithMany(cm => cm.EmailsSent)
                .HasForeignKey(se => se.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            // Defines the foreign key relationship between SendsEmails and CustomerData with Cascade Delete
            modelBuilder.Entity<SendsEmails>()
                .HasOne(se => se.CustomerData)
                .WithMany(cd => cd.EmailsSent)
                .HasForeignKey(se => se.CustomerDataId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforces a required relationship between Campaign and SendsEmails (Requered Foreign Id)
            modelBuilder.Entity<Campaign>()
                .HasMany(se => se.EmailsSent)
                .WithOne(cm => cm.Campaign)
                .HasForeignKey(cm => cm.CampaignId)
                .IsRequired();

            // Defines optional foreign key relationship between CustomerData and SendsEmails (To do so, will need the ID)
            modelBuilder.Entity<CustomerData>()
                .HasMany(se => se.EmailsSent)
                .WithOne(cd => cd.CustomerData)
                .HasForeignKey(cm => cm.CustomerDataId)
                .IsRequired(false);

            // Defines relationship between UserInfo and Campaigns where one UserInfo can have many Campaigns
            modelBuilder.Entity<UserInfo>()
                .HasMany(u => u.Campaigns)
                .WithOne(c => c.CreatedByUser)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict deletion of users if campaigns are assigned
        }
    }
}

