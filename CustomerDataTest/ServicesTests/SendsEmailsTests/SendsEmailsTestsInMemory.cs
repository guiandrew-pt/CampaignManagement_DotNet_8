using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataEnum.Enum;
using CustomerDataService.Data;
using CustomerDataService.Services;
using CustomerDataService.Services.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomerDataTest.ServicesTests.SendsEmailsTests
{
    /// <summary>
    /// Provides a suite of tests for the <see cref="SendsEmailsService"/> using an in-memory database context. 
    /// This class validates key functionalities including retrieval, filtering, pagination, 
    /// insertion, updating, and deletion of <see cref="SendsEmails"/> entities. 
    /// 
    /// Each test method is isolated and does not depend on the results of any other test.
    /// The <see cref="SeedDataAsync"/> method helps set up specific scenarios required by each test.
    /// </summary>
    public class SendsEmailsTestsInMemory
	{
        private readonly SendsEmailsService _emailService;
        private readonly CustomerDataContext _dbContext; // Store DbContext reference for seeding and cleaning up

        /// <summary>
        /// Initializes a new instance of <see cref="SendsEmailsTestsInMemory"/> with an in-memory database setup.
        /// </summary>
        public SendsEmailsTestsInMemory()
		{
            // Set up DbContext options to use an in-memory database
            DbContextOptions<CustomerDataContext> options = new DbContextOptionsBuilder<CustomerDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for isolation
                .Options;

            // Create the DbContext with the options
            _dbContext = new CustomerDataContext(options);

            // Initialize the SendsEmailsService with the DbContext
            _emailService = new SendsEmailsService(_dbContext);
        }

        /// <summary>
        /// Verifies that <see cref="SendsEmailsService.FindAllAsync"/> returns the correct page of emails in descending order by SentDate.
        /// </summary>
        [Fact]
        public async Task FindAllAsync_ReturnsPagedEmailsInMemory()
        {
            // Arrange - Seed data for multiple emails
            int campaignId = 1, customerId = 1, userId = 1;

            for (int i = 1; i <= 15; i++)
            {
                await SeedDataAsync(emailId: i, campaignId, customerId, userId);

                SendsEmails? email = await _dbContext.SendsEmails.FindAsync(i);
                email!.SentDate = DateTime.UtcNow.AddDays(-i); // Set unique dates for sorting
            }

            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);

            // Act - Retrieve the first page
            int pageNumber = 1;
            int pageSize = 5;
            List<SendsEmailsDto> result = await _emailService.FindAllAsync(pageNumber, pageSize);

            // Assert - Verify that only the correct page is returned
            result.Should().HaveCount(5); // Page size limit
            result.First().Id.Should().Be(15); // Most recent ID on page 1
            result.Last().Id.Should().Be(11); // Fifth most recent ID on page 1

            // Act - Retrieve the second page
            pageNumber = 2;
            result = await _emailService.FindAllAsync(pageNumber, pageSize);

            // Assert - Verify pagination for the second page
            result.Should().HaveCount(5); // Page size limit
            result.First().Id.Should().Be(10); // Most recent ID on page 2
            result.Last().Id.Should().Be(6); // Fifth most recent ID on page 2
        }

        /// <summary>
        /// Tests that <see cref="SendsEmailsService.FindByIdAsync"/> returns the correct email with related associations.
        /// </summary>
        [Fact]
        public async Task FindByIdAsync_ReturnsCorrectEmailWithAssociationsInMemory()
        {
            // Arrange - Seed data for email, campaign, customer, and user
            int emailId = 1;
            int campaignId = 1;
            int customerId = 1;
            int userId = 1;
            await SeedDataAsync(emailId, campaignId, customerId, userId);

            // Act - Retrieve the email by ID
            SendsEmailsDto result = await _emailService.FindByIdAsync(emailId);

            // Assert - Verify that the result matches the seeded data
            result.Should().NotBeNull();
            result.Id.Should().Be(emailId);
            result.RecipientEmail.Should().Be("customer@example.com");
            result.Subject.Should().Be("Welcome to the Summer Sale!");
            result.CampaignName.Should().Be("Summer Sale");
            result.CustomerDataName.Should().Be("Jane Smith");
            result.CreatedByEmail.Should().Be("johndoe@example.com");
        }

        /// <summary>
        /// Tests that <see cref="SendsEmailsService.FindByDateAsync"/> retrieves emails within the specified date range.
        /// </summary>
        [Fact]
        public async Task FindByDateAsync_ReturnsEmailsWithinDateRangeInMemory()
        {
            // Arrange - Seed data with different sent dates
            int emailId1 = 1, emailId2 = 2, emailId3 = 3;
            int campaignId = 1, customerId = 1, userId = 1;

            await SeedDataAsync(emailId1, campaignId, customerId, userId);
            await SeedDataAsync(emailId2, campaignId, customerId, userId);
            await SeedDataAsync(emailId3, campaignId, customerId, userId);

            // Update sent dates for range testing
            SendsEmails? email1 = await _dbContext.SendsEmails.FindAsync(emailId1);
            email1!.SentDate = DateTime.UtcNow.AddDays(-5); // 5 days ago

            SendsEmails? email2 = await _dbContext.SendsEmails.FindAsync(emailId2);
            email2!.SentDate = DateTime.UtcNow.AddDays(-2); // 2 days ago

            SendsEmails? email3 = await _dbContext.SendsEmails.FindAsync(emailId3);
            email3!.SentDate = DateTime.UtcNow; // Today

            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);

            // Define a date range: 3 days ago until now
            DateTime minDate = DateTime.UtcNow.AddDays(-3);
            DateTime maxDate = DateTime.UtcNow;

            // Act - Retrieve emails within the date range
            List<SendsEmailsDto> result = await _emailService.FindByDateAsync(minDate, maxDate);

            // Assert - Verify that only the correct emails are returned
            result.Should().HaveCount(2);
            result.Select(e => e.Id).Should().Contain(new[] { emailId2, emailId3 }); // IDs of emails within range
            result.Select(e => e.Id).Should().NotContain(emailId1); // Exclude emails outside range
        }

        /// <summary>
        /// Tests that <see cref="SendsEmailsService.FindByDateAsync"/> applies pagination to date-filtered emails.
        /// </summary>
        [Fact]
        public async Task FindByDateAsync_AppliesPaginationInMemory()
        {
            // Arrange - Seed data for 12 emails to test pagination
            int startEmailId = 1, campaignId = 1, customerId = 1, userId = 1, count = 12;

            await SeedDataAsync(startEmailId, campaignId, customerId, userId, count);


            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);

            // Act - Retrieve a specific page
            int pageNumber = 2;
            int pageSize = 5;
            List<SendsEmailsDto> result = await _emailService.FindByDateAsync(null, null, pageNumber, pageSize);

            // Assert - Verify pagination is applied correctly
            result.Should().HaveCount(5); // Page size limit
            //result.First().Id.Should().Be(6); // First item on page 2 with 5 items per page
            //result.Last().Id.Should().Be(10); // Last item on page 2 with 5 items per page

            // Assert - Verify pagination: page 2 should contain items with IDs 7-11 based on the SentDate order
            result.Select(e => e.Id).Should().Equal(7, 6, 5, 4, 3); // Expected IDs based on descending SentDate order
        }

        /// <summary>
        /// Tests the <see cref="SendsEmailsService.FindByDateCustomerDataIdAsync"/> method
        /// to ensure that it applies filters for CustomerDataId and date range correctly, 
        /// and also applies pagination based on specified page number and page size.
        /// </summary>
        [Fact]
        public async Task FindByDateCustomerDataIdAsync_FiltersAndAppliesPaginationInMemory()
        {
            // Arrange 
            int customerId = 1; // The CustomerDataId filter to test
            int campaignId = 1; // Related CampaignId for the test data
            int userId = 1; // UserId for the test data
            int count = 5; // Number of emails to seed

            // Seed 5 emails to ensure sufficient data for testing pagination and filtering
            for (int i = 1; i <= 5; i++)
            {
                await SeedDataAsync(i, campaignId, customerId, userId, count);
            }

            DateTime? minDate = DateTime.UtcNow.AddDays(-5); // Adjust range as needed
            DateTime? maxDate = DateTime.UtcNow;

            // Act
            List<SendsEmailsDto> result = await _emailService.FindByDateCustomerDataIdAsync(
                customerId, // Apply CustomerDataId filter
                minDate, // Minimum date filter
                maxDate, // Maximum date filter
                pageNumber: 1,
                pageSize: 5
            );

            // Assert
            result.Should().HaveCount(5, "because 5 emails match the filters"); // Validate page size

            // Verify that all results match the specified CustomerDataId
            result.All(e => e.CustomerDataId == customerId).Should().BeTrue("all emails should belong to the specified CustomerDataId");

            // Verify that all results are within the date range
            result.All(e => e.SentDate >= minDate && e.SentDate <= maxDate).Should().BeTrue("all emails should be within the specified date range");

            // Verify that the results are ordered by SentDate in descending order
            result.Should().BeInDescendingOrder(e => e.SentDate, "emails should be ordered by SentDate in descending order");
        }

        /// <summary>
        /// Verifies that <see cref="SendsEmailsService.InsertAsync"/> successfully inserts
        /// a valid <see cref="SendsEmails"/> entity and that all properties are saved correctly.
        /// </summary>
        [Fact]
        public async Task InsertAsync_SuccessfullyInsertsValidSendsEmailsInMemory()
        {
            // Arrange
            int campaignId = 1, customerId = 1, userId = 1;
            await SeedDataAsync(1, campaignId, customerId, userId); // Seed related Campaign and CustomerData

            SendsEmails newEmail = new SendsEmails
            {
                Id = 100,
                RecipientEmail = "newcustomer@example.com",
                Subject = "New Campaign Offer!",
                Content = "Exclusive 30% off for our valued customers!",
                SentDate = DateTime.UtcNow,
                Status = Status.Sent,
                CampaignId = campaignId,
                CustomerDataId = customerId // Ensure it matches seeded CustomerData
            };

            // Act
            await _emailService.InsertAsync(newEmail);

            // Assert
            SendsEmails? insertedEmail = await _dbContext.SendsEmails.FindAsync(newEmail.Id);
            insertedEmail.Should().NotBeNull();
            insertedEmail!.RecipientEmail.Should().Be(newEmail.RecipientEmail);
            insertedEmail.Subject.Should().Be(newEmail.Subject);
            insertedEmail.CampaignId.Should().Be(newEmail.CampaignId);
            insertedEmail.CustomerDataId.Should().Be(newEmail.CustomerDataId);
        }

        /// <summary>
        /// Verifies that <see cref="SendsEmailsService.InsertAsync"/> throws <see cref="NotFoundException"/>
        /// when an invalid Campaign ID is used in a <see cref="SendsEmails"/> entity.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsNotFoundException_SendsEmailsWhenCampaignDoesNotExistInMemory()
        {
            // Arrange
            int invalidCampaignId = 999; // Non-existent Campaign ID, related with SendsEmails
            int customerId = 1, userId = 1;
            await SeedDataAsync(1, campaignId: 1, customerId, userId); // Seed related CustomerData but no Campaign with ID 999

            SendsEmails invalidEmail = new SendsEmails
            {
                Id = 101,
                RecipientEmail = "missingcampaign@example.com",
                Subject = "Missing Campaign!",
                Content = "Campaign ID doesn't exist.",
                SentDate = DateTime.UtcNow,
                Status = Status.Failed,
                CampaignId = invalidCampaignId, // Invalid Campaign
                CustomerDataId = customerId
            };

            // Act
            Func<Task> act = () => _emailService.InsertAsync(invalidEmail);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Campaign with ID {invalidCampaignId} not found!");
        }

        /// <summary>
        /// Tests the <see cref="SendsEmailsService.InsertAsync"/> method to ensure that 
        /// it throws a <see cref="NotFoundException"/> when attempting to insert a 
        /// <see cref="SendsEmails"/> record with a non-existent CustomerDataId.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsNotFoundException_SendsEmailsWhenCustomerDataIdDoesNotExistInMemory()
        {
            // Arrange
            int campaignId = 1;                // Valid CampaignId for the email
            int invalidCustomerId = 999;       // Non-existent CustomerDataId to trigger NotFoundException
            int userId = 1;                    // Valid UserId for the campaign creator

            // Seed data for related entities (Campaign, but not CustomerData with ID 999)
            await SeedDataAsync(emailId: 1, campaignId, customerId: 1, userId);

            // Create a SendsEmails object with an invalid CustomerDataId
            SendsEmails invalidEmail = new SendsEmails
            {
                Id = 102,
                RecipientEmail = "missingcustomer@example.com",
                Subject = "Missing Customer!",
                Content = "CustomerData ID doesn't exist.",
                SentDate = DateTime.UtcNow,
                Status = Status.Failed,
                CampaignId = campaignId, // Valid CampaignId
                CustomerDataId = invalidCustomerId // Invalid CustomerDataId to trigger exception
            };

            // Act
            Func<Task> act = () => _emailService.InsertAsync(invalidEmail);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"CustomerData with ID {invalidCustomerId} not found!");
        }

        /// <summary>
        /// Verifies that the InsertAsync method throws a NotFoundException when a null SendsEmails object is provided.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsNotFoundException_WhenSendsEmailsIsNullInMemory()
        {
            // Act
            // Attempt to insert a null SendsEmails object
            Func<Task> act = () => _emailService.InsertAsync(null!);

            // Assert
            // Expect a NotFoundException with a specific message
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("SendsEmails is empty or null!");
        }

        /// <summary>
        /// Tests that the UpdateAsync method successfully updates an existing SendsEmails entity in the database.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdatesExistingSendsEmailsInMemory()
        {
            // Arrange
            // Seed initial data for the SendsEmails entry
            int emailId = 1, campaignId = 1, customerId = 1, userId = 1;
            await SeedDataAsync(emailId, campaignId, customerId, userId);

            // Create an updated version of the SendsEmails entity
            SendsEmails updatedEmail = new SendsEmails
            {
                Id = emailId,
                RecipientEmail = "updatedcustomer@example.com",
                Subject = "Updated Campaign Offer!",
                Content = "Updated offer with exclusive benefits.",
                SentDate = DateTime.UtcNow.AddDays(-1), // Updated sent date
                Status = Status.Failed,
                CampaignId = campaignId,
                CustomerDataId = customerId
            };

            // Act
            // Perform the update operation
            await _emailService.UpdateAsync(updatedEmail);

            // Assert
            // Retrieve the updated SendsEmails entry from the database and verify that all fields match
            SendsEmails? emailInDb = await _dbContext.SendsEmails.FindAsync(emailId);
            emailInDb.Should().NotBeNull();
            emailInDb!.RecipientEmail.Should().Be(updatedEmail.RecipientEmail);
            emailInDb.Subject.Should().Be(updatedEmail.Subject);
            emailInDb.Content.Should().Be(updatedEmail.Content);
            emailInDb.SentDate.Should().BeCloseTo(updatedEmail.SentDate, TimeSpan.FromMilliseconds(100)); // Account for potential precision differences
            emailInDb.Status.Should().Be(updatedEmail.Status);
        }

        /// <summary>
        /// Verifies that the UpdateAsync method throws a NotFoundException when the SendsEmails entity does not exist in the database.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ThrowsNotFoundException_WhenSendsEmailsDoesNotExistInMemory()
        {
            // Arrange
            // Create a non-existent SendsEmails entity with an invalid ID
            SendsEmails nonExistentEmail = new SendsEmails
            {
                Id = 999, // Non-existent ID
                RecipientEmail = "nonexistent@example.com",
                Subject = "Non-existent Email",
                Content = "This email does not exist in the database.",
                SentDate = DateTime.UtcNow,
                Status = Status.Sent,
                CampaignId = 1, // Assume valid campaign
                CustomerDataId = 1 // Assume valid customer
            };

            // Act
            // Attempt to update the non-existent SendsEmails entry
            Func<Task> act = () => _emailService.UpdateAsync(nonExistentEmail);

            // Assert
            // Expect a NotFoundException with a specific message
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Id(SendsEmails) not found!");
        }

        /// <summary>
        /// Verifies that the UpdateAsync method throws a NotFoundException when an invalid CampaignId is provided.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ThrowsNotFoundException_SendsEmailsWhenCampaignIdIsInvalidInMemory()
        {
            // Arrange
            // Seed initial data with a valid SendsEmails entry
            int emailId = 1, invalidCampaignId = 999, customerId = 1, userId = 1;
            await SeedDataAsync(emailId, campaignId: 1, customerId, userId); // Valid initial data

            // Create an updated version of the SendsEmails entity with an invalid CampaignId
            SendsEmails updatedEmail = new SendsEmails
            {
                Id = emailId,
                RecipientEmail = "validcustomer@example.com",
                Subject = "Valid Subject",
                Content = "Valid Content",
                SentDate = DateTime.UtcNow,
                Status = Status.Failed,
                CampaignId = invalidCampaignId, // Invalid campaign ID
                CustomerDataId = customerId
            };

            // Act
            // Attempt to update the SendsEmails entry with the invalid CampaignId
            Func<Task> act = () => _emailService.UpdateAsync(updatedEmail);

            // Assert
            // Expect a NotFoundException with a specific message
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Campaign not found!");
        }

        /// <summary>
        /// Verifies that <see cref="SendsEmailsService.UpdateAsync"/> throws <see cref="NotFoundException"/>
        /// when updating a <see cref="SendsEmails"/> entity with an invalid CustomerData ID.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ThrowsNotFoundException_SendsEmailsWhenCustomerDataIdIsInvalidInMemory()
        {
            // Arrange
            int emailId = 1, campaignId = 1, invalidCustomerId = 999, userId = 1;
            await SeedDataAsync(emailId, campaignId, customerId: 1, userId); // Valid initial data

            SendsEmails updatedEmail = new SendsEmails
            {
                Id = emailId,
                RecipientEmail = "validcustomer@example.com",
                Subject = "Valid Subject",
                Content = "Valid Content",
                SentDate = DateTime.UtcNow,
                Status = Status.Sent,
                CampaignId = campaignId,
                CustomerDataId = invalidCustomerId // Invalid customer
            };

            // Act
            Func<Task> act = () => _emailService.UpdateAsync(updatedEmail);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("CustomerData not found!");
        }

        /// <summary>
        /// Verifies that the RemoveAsync method successfully deletes an existing SendsEmails entity from the database.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_DeletesExistingSendsEmailsInMemory()
        {
            // Arrange - Seed the database with a sendsEmails having Id = 1
            int emailId = 1, campaignId = 1, customerId = 1, userId = 1;
            await SeedDataAsync(emailId, campaignId, customerId, userId);

            // Act - Call the RemoveAsync method to delete the SendsEmails entry
            await _emailService.RemoveAsync(emailId);

            // Assert - Verify that the sendsEmails was deleted
            SendsEmails? result = await _dbContext.SendsEmails.FindAsync(emailId);
            Assert.Null(result); // The result should be null since the entry was deleted
        }

        /// <summary>
        /// Verifies that the RemoveAsync method throws a NotFoundException when attempting to delete a non-existent SendsEmails entity.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_ThrowsNotFoundException_WhenSendsEmailsDoesNotExistInMemory()
        {
            // Arrange - Specify a non-existent email ID
            int nonExistentSendsEmailsId = 999;

            // Act & Assert
            // Attempt to call RemoveAsync with a non-existent ID and expect a NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(
                () => _emailService.RemoveAsync(nonExistentSendsEmailsId));
        }

        // ===========
        /// <summary>
        /// Helper method to seed test data into the in-memory database for testing purposes.
        /// </summary>
        /// <param name="emailId">The ID for the SendsEmails entry.</param>
        /// <param name="campaignId">The Campaign ID associated with the email.</param>
        /// <param name="customerId">The CustomerData ID associated with the email.</param>
        /// <param name="userId">The UserInfo ID associated with the Campaign.</param>
        /// <param name="count">The number of emails to seed.</param>
        private async Task SeedDataAsync(int emailId, int campaignId, int customerId, int userId, int count = 1)
        {
            // Seed UserInfo (Campaign Creator)
            UserInfo? user = new UserInfo
            {
                Id = userId,
                Username = "CampaignCreator",
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Roles = new[] { "Admin" },
                PasswordHash = "password",
                Token = "dummy-token"
            };

            if (!_dbContext.UserInfo.Any(u => u.Id == userId))
            {
                await _dbContext.UserInfo.AddAsync(user);
            }

            // Seed Campaign
            Campaign? campaign = new Campaign
            {
                Id = campaignId,
                CampaignName = "Summer Sale",
                Description = "End of summer discounts",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10),
                IsActive = true,
                CreatedByUserId = userId
            };

            if (!_dbContext.Campaign.Any(c => c.Id == campaignId))
            {
                await _dbContext.Campaign.AddAsync(campaign);
            }

            // Seed CustomerData
            CustomerData? customer = new CustomerData
            {
                Id = customerId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "janesmith@example.com",
                Phone = "(123) 456-7890",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            if (!_dbContext.CustomerData.Any(c => c.Id == customerId))
            {
                await _dbContext.CustomerData.AddAsync(customer);
            }

            // Seed SendsEmails
            for (int i = 0; i < count; i++)  // Create exactly 5 emails
            {
                SendsEmails email = new SendsEmails
                {
                    Id = emailId + i,
                    RecipientEmail = "customer@example.com",
                    Subject = "Welcome to the Summer Sale!",
                    Content = "Enjoy up to 50% off!",
                    SentDate = DateTime.UtcNow.AddDays(-i), // Unique SentDate for descending order
                    Status = Status.Sent,
                    CampaignId = campaignId,
                    CustomerDataId = customerId
                };

                if (!_dbContext.SendsEmails.Any(e => e.Id == email.Id))
                {
                    await _dbContext.SendsEmails.AddAsync(email);
                }
            }

            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);
        }

        /// <summary>
        /// Detaches all tracked entities from the context to avoid state conflicts in subsequent operations.
        /// Detaches all entities from the context to prevent caching or tracking issues between tests.
        /// </summary>
        /// <param name="context">The database context instance.</param>
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

