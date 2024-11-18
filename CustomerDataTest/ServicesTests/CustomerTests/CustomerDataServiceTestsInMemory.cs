using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Data;
using CustomerDataService.Services.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomerDataTest.ServicesTests.CustomerTests
{
    /// <summary>
    /// Unit tests for CustomerDataService using an in-memory database.
    /// These tests validate CRUD operations and handle edge cases for customers.
    /// </summary>
    public class CustomerDataServiceTestsInMemory
	{
        private readonly CustomerDataService.Services.CustomerDataService _customerService;
        private readonly CustomerDataContext _dbContext; // Store DbContext reference for seeding and cleaning up

        /// <summary>
        /// Initializes the test class with an in-memory database and CustomerDataService instance.
        /// </summary>
        public CustomerDataServiceTestsInMemory()
		{
            // Set up DbContext options to use an in-memory database
            DbContextOptions<CustomerDataContext> options = new DbContextOptionsBuilder<CustomerDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for isolation
                .Options;

            // Create the DbContext with the options
            _dbContext = new CustomerDataContext(options);

            // Initialize the CustomerService with the DbContext
            _customerService = new CustomerDataService.Services.CustomerDataService(_dbContext);
        }

        /// <summary>
        /// Tests that FindAllAsync retrieves all customers in the database.
        /// </summary>
        [Fact]
        public async Task FindAllAsync_ReturnsAllCustomersInMemory()
        {
            // Seed initial data with user and customer
            await SeedDataAsync(customerId: 1, firstName: "Zeus");

            // Add a second customer with
            await SeedDataAsync(customerId: 2, firstName: "Ades");

            // Act - Call the FindAllAsync method
            List<CustomerData> result = await _customerService.FindAllAsync();

            // Assert - Check that both customers are returned
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Additional assertions for customers
            // Check individual customers
            CustomerData? firstCustomer = result.FirstOrDefault(c => c.Id == 1);
            Assert.NotNull(firstCustomer);
            Assert.Equal("Zeus", firstCustomer.FirstName);

            CustomerData? secondCustomer = result.FirstOrDefault(c => c.Id == 2);
            Assert.NotNull(secondCustomer);
            Assert.Equal("Ades", secondCustomer.FirstName);
        }

        /// <summary>
        /// Tests that GetCustomerByIdAsync retrieves the correct customer by ID.
        /// </summary>
        [Fact]
        public async Task GetCustomerByIdAsync_ReturnsCorrectCustomerInMemory()
        {
            // Arrange - Set up in-memory database and seed data
            // Seed the database with a customer having Id = 1
            await SeedDataAsync(customerId: 1, firstName: "Rogers");

            // Act - Retrieve the customer by ID
            CustomerData? result = await _customerService.FindByIdAsync(1);

            // Assert - Check that the customer has the expected properties
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Rogers", result.FirstName);
        }

        /// <summary>
        /// Tests that InsertAsync successfully adds a new customer to the database.
        /// </summary>
        [Fact]
        public async Task InsertAsync_AddsNewCustomerSuccessfullyInMemory()
        {
            // Arrange - create a new customer object to insert
            CustomerData? customer = new CustomerData
            {
                FirstName = "Bruce",
                LastName = "Lee",
                Email = "bruce@example.com",
                Phone = "(123) 456-7770",
                DateOfBirth = new DateTime(1998, 4, 1)
            };

            // Act - Call InsertAsync
            await _customerService.InsertAsync(customer);

            // Assert - Verify that the customer was added successfully
            CustomerData? result = await _dbContext.CustomerData.FirstOrDefaultAsync(c => c.FirstName == "Bruce");

            //Assert.NotNull(result);
            //Assert.Equal("Bruce", result.FirstName);
            //Assert.Equal("bruce@example.com", result.Email);

            // Using FluentAssertions
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Bruce");
            result.LastName.Should().Be("Lee");
            result.Email.Should().Be("bruce@example.com");
            result.Phone.Should().Be("(123) 456-7770");
            result.DateOfBirth.Should().Be(new DateTime(1998, 4, 1));
        }

        /// <summary>
        /// Tests that InsertAsync throws DbUpdateException when a customer lacks a phone number.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsDbUpdateException_WhenPhoneNotExistInMemory()
        {
            // Arrange - Create a customer with a missing Phone property
            CustomerData? customer = new CustomerData
            {
                FirstName = "Bruce",
                LastName = "Lee",
                Email = "bruce@example.com",
                DateOfBirth = new DateTime(1998, 4, 1)
                // Missing Phone
            };

            // Act & Assert - Expect a DbUpdateException due to the missing Phone property
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _customerService.InsertAsync(customer));
        }

        /// <summary>
        /// Tests that InsertAsync throws an InvalidOperationException when attempting to add a customer with a duplicate email.
        /// </summary>
        [Fact]
        public async Task InsertAsync_ThrowsDbUpdateException_WhenDuplicateEmailExistsInMemory()
        {
            // Arrange - Seed the initial customer data with a specific email
            await SeedDataAsync(customerId: 1, firstName: "Original", email: "unique@example.com");

            // Attempt to add another customer with the same email
            CustomerData duplicateCustomer = new CustomerData
            {
                FirstName = "Duplicate",
                LastName = "User",
                Email = "unique@example.com", // Duplicate email
                Phone = "(555) 123-4567",
                DateOfBirth = new DateTime(1995, 5, 15)
            };

            // Act & Assert
            Func<Task> action = async () => await _customerService.InsertAsync(duplicateCustomer);

            // Assert that an InvalidOperationException is thrown for duplicate email
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A customer with this email already exists.");
        }

        /// <summary>
        /// Tests that UpdateAsync successfully updates an existing customer's information.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_UpdatesCustomerSuccessfullyInMemory()
        {
            // Arrange - Seed initial data
            int customerId = 1;
            string originalFirstName = "Jimmy";
            await SeedDataAsync(customerId, originalFirstName);

            // Prepare an updated customer object with the same ID and different details
            CustomerData updatedCustomer = new CustomerData
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Jones",
                Email = "jones@example.com",
                Phone = "(123) 456-8878",
                DateOfBirth = new DateTime(1989, 1, 7)
            };

            // Act - Call UpdateAsync to update the customer in the database
            await _customerService.UpdateAsync(updatedCustomer);

            // Assert - Verify that the customer has been updated
            CustomerData? result = await _dbContext.CustomerData.FindAsync(customerId);
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Jones", result.LastName);
            Assert.Equal(updatedCustomer.Email, result.Email);
            Assert.Equal(updatedCustomer.Phone, result.Phone);
            Assert.Equal(updatedCustomer.DateOfBirth, result.DateOfBirth);
        }

        /// <summary>
        /// Tests that UpdateAsync throws NotFoundException when attempting to update a non-existent customer.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ThrowsNotFoundException_WhenCustomerDoesNotExistInMemory()
        {
            // Arrange - Prepare an updated customer object with an ID that doesn't exist
            CustomerData updatedCustomer = new CustomerData
            {
                Id = 999, // Assuming this ID doesn't exist // Non-existent ID
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "(123) 456-7890",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            // Act & Assert - Expect a NotFoundException
            Func<Task> action = async () => await _customerService.UpdateAsync(updatedCustomer);

            // Using FluentAssertions for exception assertion
            await action.Should().ThrowAsync<NotFoundException>()
                  .WithMessage($"CustomerData with ID {updatedCustomer.Id} not found!");
        }

        /// <summary>
        /// Tests that UpdateAsync throws InvalidOperationException when when duplicate email exists.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenDuplicateEmailExists()
        {
            // Arrange - Seed two customers with unique emails
            await SeedDataAsync(customerId: 1, firstName: "First", email: "original@example.com");
            await SeedDataAsync(customerId: 2, firstName: "Second", email: "duplicate@example.com");

            // Attempt to update the first customer to use the second customer's email
            CustomerData customerToUpdate = new CustomerData
            {
                Id = 1,
                FirstName = "First Updated",
                LastName = "User",
                Email = "duplicate@example.com", // Email in use by another customer
                Phone = "(555) 987-6543",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act & Assert
            Func<Task> action = async () => await _customerService.UpdateAsync(customerToUpdate);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This email is already associated with another customer.");
        }

        /// <summary>
        /// Tests that RemoveAsync successfully deletes an existing customer from the database.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_DeletesExistingCustomerInMemory()
        {
            // Arrange - Seed the database with a customer having Id = 1
            int customerId = 1;
            await SeedDataAsync(customerId, "Customer to Delete");

            // Act - Call RemoveAsync to delete the customer
            await _customerService.RemoveAsync(customerId);

            // Assert - Verify that the customer was deleted
            CustomerData? result = await _dbContext.CustomerData.FindAsync(customerId);
            Assert.Null(result); // Should be null since it was deleted
        }

        /// <summary>
        /// Tests that RemoveAsync throws NotFoundException when attempting to delete a non-existent customer.
        /// </summary>
        [Fact]
        public async Task RemoveAsync_ThrowsNotFoundException_WhenCustomerDoesNotExistInMemory()
        {
            // Arrange - Specify a non-existent customer ID
            int nonExistentCustomerId = 999;

            // Act & Assert - Expect a NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(
                () => _customerService.RemoveAsync(nonExistentCustomerId));
        }

        // ===========
        /// <summary>
        /// Seeds the database with test data for customers to ensure controlled conditions in unit tests.
        /// </summary>
        /// <param name="customerId">Customer ID.</param>
        /// <param name="firstName">First name.</param>
        /// <param name="email">Customer email.</param>
        private async Task SeedDataAsync(int customerId, string firstName, string email = "john@example.com")
        {
            // Add or update the customer in case it already exists to prevent conflicts
            CustomerData? customer = new CustomerData
            {
                Id = customerId,
                FirstName = firstName,
                LastName = "Rambo",
                Email = email,
                Phone = "(123) 456-7890",
                DateOfBirth = new DateTime(2006, 1, 1)
        };

            await _dbContext.CustomerData.AddAsync(customer);
            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);
        }

        /// <summary>
        /// Detaches all entities tracked by the DbContext to simulate fresh database operations.
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

