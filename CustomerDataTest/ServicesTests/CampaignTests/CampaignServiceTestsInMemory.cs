using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataEnum.Enum;
using CustomerDataService.Data;
using CustomerDataService.Services;
using CustomerDataService.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomerDataTest.ServicesTests.CampaignTests
{
    /// <summary>
    /// Unit tests for the CampaignService using an in-memory database.
    /// </summary>
    public class CampaignServiceTestsInMemory
	{
        private readonly CampaignService _campaignService;
        private readonly CustomerDataContext _dbContext; // Store DbContext reference for seeding and cleaning up


        /// <summary>
        /// Initializes the test class with an in-memory database and CampaignService instance.
        /// </summary>
        public CampaignServiceTestsInMemory()
		{
            // Set up DbContext options to use an in-memory database
            DbContextOptions<CustomerDataContext> options = new DbContextOptionsBuilder<CustomerDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for isolation
                .Options;

            // Create the DbContext with the options
            _dbContext = new CustomerDataContext(options);

            // Initialize the CampaignService with the DbContext
            _campaignService = new CampaignService(_dbContext);
        }

        /// <summary>
        /// Verifies that the FindAllAsync method retrieves all campaigns with associated data from the database.
        /// </summary>
        [Fact]
        public async Task FindAllAsync_ReturnsAllCampaignsWithAssociationsInMemory()
        {
            // Arrange - Seed initial data with user and two campaigns
            // Seed initial data with user and campaign
            await SeedDataAsync(campaignId: 1, campaignName: "Winter Promo!");
            // Add a second campaign with a new user (or the same user if desired)
            await SeedDataAsync(campaignId: 2, campaignName: "Summer Sale");

            // Act - Call the FindAllAsync method to retrieve all campaigns
            List<CampaignDto> result = await _campaignService.FindAllAsync();

            // Assert - Check that both campaigns are returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Additional assertions for campaigns properties
            CampaignDto? firstCampaign = result.FirstOrDefault(c => c.Id == 1);
            Assert.NotNull(firstCampaign);
            Assert.Equal("Winter Promo!", firstCampaign.CampaignName);

            CampaignDto? secondCampaign = result.FirstOrDefault(c => c.Id == 2);
            Assert.NotNull(secondCampaign);
            Assert.Equal("Summer Sale", secondCampaign.CampaignName);
        }

        /// <summary>
        /// Verifies that the FindByIdAsync method retrieves the correct campaign by ID.
        /// </summary>
        [Fact]
        public async Task GetCampaignByIdAsync_ReturnsCorrectCampaignInMemory()
        {
            // Arrange - Set up in-memory database and seed data
            // Seed the database with a campaign having Id = 1
            await SeedDataAsync(campaignId: 1, campaignName: "Winter Promo!");

            // Act - Retrieve the campaign by ID
            CampaignDto? result = await _campaignService.FindByIdAsync(1);

            // Assert - Check that the campaign has the expected properties
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Winter Promo!", result.CampaignName);
        }

        /// <summary>
        /// Verifies that the InsertAsync method adds a new campaign to the database successfully.
        /// </summary>
        [Fact]
        public async Task InsertAsync_AddsNewCampaignSuccessfullyInMemory()
        {
            // Arrange - Seed a user in the database to associate with the campaign
            int userId = 1;
            await SeedUserAsync(userId, "TestUser");

            // Create a new campaign object to insert
            Campaign? campaign = new Campaign
            {
                CampaignName = "Spring Promo!",
                Description = "Seasonal promotion campaign",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            };

            // Act - Call InsertAsync to add the new campaign
            await _campaignService.InsertAsync(campaign, userId);

            // Assert - Verify that the campaign was added successfully
            Campaign? result = await _dbContext.Campaign.FirstOrDefaultAsync(c => c.CampaignName == "Spring Promo!");
            Assert.NotNull(result);
            Assert.Equal("Spring Promo!", result.CampaignName);
            Assert.Equal(userId, result.CreatedByUserId);
        }

        /// <summary>
        /// Verifies that the InsertAsync method throws an UnauthorizedAccessException 
        /// when attempting to insert a campaign with a non-existent user ID.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsUnauthorizedAccessException_WhenUserDoesNotExistInMemory()
        {
            // Arrange - Create a campaign with a non-existent user ID
            int nonExistentUserId = 99;
            Campaign? campaign = new Campaign
            {
                CampaignName = "Unauthorized Campaign",
                Description = "This should fail",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(15),
                IsActive = true
            };

            // Act & Assert - Expect an UnauthorizedAccessException when calling InsertAsync
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _campaignService.InsertAsync(campaign, nonExistentUserId));
        }

        /// <summary>
        /// Verifies that the UpdateAsync method successfully updates an existing campaign in the database.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_UpdatesCampaignSuccessfullyInMemory()
        {
            // Arrange - Seed initial data for the campaign
            int campaignId = 1;
            string originalName = "Original Campaign Name";
            await SeedDataAsync(campaignId, originalName);

            // Prepare an updated campaign object with the same ID and different details
            Campaign updatedCampaign = new Campaign
            {
                Id = campaignId,
                CampaignName = "Updated Campaign Name",
                Description = "Updated description.",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                IsActive = false // Toggle active status
            };

            // Act - Call UpdateAsync to update the campaign in the database
            await _campaignService.UpdateAsync(updatedCampaign);

            // Assert - Verify that the campaign details were updated
            Campaign? result = await _dbContext.Campaign.FindAsync(campaignId);
            Assert.NotNull(result);
            Assert.Equal("Updated Campaign Name", result.CampaignName);
            Assert.Equal("Updated description.", result.Description);
            Assert.Equal(updatedCampaign.StartDate, result.StartDate);
            Assert.Equal(updatedCampaign.EndDate, result.EndDate);
            Assert.Equal(updatedCampaign.IsActive, result.IsActive);
        }

        /// <summary>
        /// Verifies that the RemoveAsync method successfully deletes an existing campaign from the database.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_DeletesExistingCampaignInMemory()
        {
            // Arrange - Seed the database with a campaign(campaign to delete) having Id = 1
            int campaignId = 1;
            await SeedDataAsync(campaignId, "Campaign to Delete");

            // Act - Call RemoveAsync to delete the campaign
            await _campaignService.RemoveAsync(campaignId);

            // Assert - Verify that the campaign was deleted
            Campaign? result = await _dbContext.Campaign.FindAsync(campaignId);
            Assert.Null(result); // Should be null since it was deleted
        }

        /// <summary>
        /// Verifies that the RemoveAsync method throws a NotFoundException 
        /// when attempting to delete a non-existent campaign.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_ThrowsNotFoundException_WhenCampaignDoesNotExistInMemory()
        {
            // Arrange - Specify a non-existent campaign ID
            int nonExistentCampaignId = 999;

            // Act & Assert - Expect a NotFoundException when calling RemoveAsync
            await Assert.ThrowsAsync<NotFoundException>(
                () => _campaignService.RemoveAsync(nonExistentCampaignId));
        }

        /// <summary>
        /// Verifies that the RemoveAsync method throws an IntegrityException 
        /// when attempting to delete a campaign that has related emails.
        /// </summary>
        // NOTE: to do this test remove the comments in the RemoveAsync method in the CampaignService
        [Fact]
        public async Task RemoveAsync_ThrowsIntegrityException_WhenCampaignHasRelatedEmailsInMemory()
        {
            // Arrange - Seed a campaign and associate it with related SendsEmails data
            int campaignId = 1;
            await SeedDataAsync(campaignId, "Campaign with Emails");
            await SeedEmailAsync(campaignId, "recipient@example.com", "Test Subject");

            // Act & Assert - Expect an IntegrityException due to the foreign key relationship(related SendsEmails)
            await Assert.ThrowsAsync<IntegrityException>(
                () => _campaignService.RemoveAsync(campaignId));
        }

        // ===========
        /// <summary>
        /// Seeds a campaign and its associated user into the in-memory database.
        /// </summary>
        /// <param name="campaignId">The ID of the campaign to seed.</param>
        /// <param name="campaignName">The name of the campaign to seed.</param>
        private async Task SeedDataAsync(int campaignId, string campaignName)
        {
            // Add a user to satisfy the CreatedByUser relationship
            UserInfo? user = new UserInfo
            {
                Id = 1,
                Username = "TestUser",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Roles = new[] { "Admin" },
                PasswordHash = "password",
                Token = "dummy-token"
            };

            if (!_dbContext.UserInfo.Any(u => u.Id == user.Id))
            {
                await _dbContext.UserInfo.AddAsync(user);
            }

            // Add or update the user in case it already exists to prevent conflicts
            Campaign? campaign = new Campaign
            {
                Id = campaignId,
                CampaignName = campaignName,
                Description = "Test the campaign with data.",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                CreatedByUserId = user.Id
            };

            await _dbContext.Campaign.AddAsync(campaign);
            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);
        }

        /// <summary>
        /// Seeds a user into the in-memory database.
        /// </summary>
        /// <param name="userId">The ID of the user to seed.</param>
        /// <param name="username">The username of the user to seed.</param>
        // Helper method to seed a user in the database
        private async Task SeedUserAsync(int userId, string username)
        {
            UserInfo? user = new UserInfo
            {
                Id = userId,
                Username = username,
                Email = $"{username.ToLower()}@example.com",
                FirstName = "John",
                LastName = "Doe",
                Roles = new[] { "Admin" },
                PasswordHash = "password",
                Token = "dummy-token"
            };

            if (!_dbContext.UserInfo.Any(u => u.Id == user.Id))
            {
                await _dbContext.UserInfo.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                DetachAllEntities(_dbContext);
            }
        }

        /// <summary>
        /// Seeds a SendsEmails record into the in-memory database.
        /// </summary>
        /// <param name="campaignId">The ID of the campaign associated with the email.</param>
        /// <param name="recipientEmail">The recipient email address.</param>
        /// <param name="subject">The subject of the email.</param>
        private async Task SeedEmailAsync(int campaignId, string recipientEmail, string subject)
        {
            SendsEmails email = new SendsEmails
            {
                RecipientEmail = recipientEmail,
                Subject = subject,
                Content = "Email content",
                SentDate = DateTime.UtcNow,
                Status = Status.Sent,
                CampaignId = campaignId // Associate email with the campaign
            };

            await _dbContext.SendsEmails.AddAsync(email);
            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);
        }

        /// <summary>
        /// Detaches all tracked entities from the context to avoid state conflicts in subsequent operations.
        /// </summary>
        /// <param name="context">The DbContext to clear tracked entities from.</param>
        private static void DetachAllEntities(DbContext context)
        {
            List<EntityEntry>? entries = context.ChangeTracker.Entries().ToList();
            foreach (EntityEntry? entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}

