using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Data;
using CustomerDataService.Services;
using CustomerDataService.Services.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomerDataTest.ServicesTests.UserTests
{
    /// <summary>
    /// Unit tests for the UserService class, using an in-memory database for isolation.
    /// </summary>
    public class UserInfoServiceTestInMemory
	{
        private readonly UserService _userService;
        private readonly CustomerDataContext _dbContext; // Store DbContext reference for seeding and cleaning up

        /// <summary>
        /// Initializes a new instance of <see cref="UserInfoServiceTestInMemory"/> with an in-memory database setup.
        /// </summary>
        public UserInfoServiceTestInMemory()
		{
            // Set up DbContext options to use an in-memory database
            DbContextOptions<CustomerDataContext> options = new DbContextOptionsBuilder<CustomerDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for isolation
                .Options;

            // Create the DbContext with the options
            _dbContext = new CustomerDataContext(options);

            // Initialize the UserService with the DbContext
            _userService = new UserService(_dbContext);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsersAsync"/> returns all users in the database.
        /// </summary>
        [Fact]
        public async Task FindAllAsync_ReturnsallUsersInMemory()
        {
            // Arrange - Seed data for two users
            await SeedDataAsync(userId: 1, userName: "UserOne", email: "mail1@example.com", password: "password1");
            await SeedDataAsync(userId: 2, userName: "UserTwo", email: "mail2@example.com", password: "password2");

            // Act - Call the GetAllUsersAsync method in UserService
            List<UserDto> result = await _userService.GetAllUsersAsync();

            // Assert - Verify that the result contains both seeded users
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Expect two users

            // Verify individual user details for correctness
            result.Should().ContainSingle(user => user.Id == 1 && user.Username == "UserOne" && user.Email == "mail1@example.com");
            result.Should().ContainSingle(user => user.Id == 2 && user.Username == "UserTwo" && user.Email == "mail2@example.com");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserByIdAsync"/> returns the correct user when queried by ID.
        /// </summary>
        [Fact]
        public async Task FindByIdAsync_ReturnsCorrectUserInMemory()
        {
            // Arrange - Seed data for user
            int userId = 1;
            string userName = "UserOne", email = "mail1@example.com", password = "password1";
            await SeedDataAsync(userId, userName, email, password);

            // Act - Retrieve the user by ID
            UserInfo result = await _userService.GetUserByIdAsync(userId);

            // Assert - Verify that the result matches the seeded data
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Username.Should().Be("UserOne");
            result.Email.Should().Be("mail1@example.com");
            result.PasswordHash.Should().Be("password1");
        }

        /// <summary>
        /// Verifies that a valid user can be successfully inserted using <see cref="UserService.InsertUserAsync"/>.
        /// </summary>
        [Fact]
        public async Task InsertUserAsync_SuccessfullyInsertsValidUserInMemory()
        {
            // Arrange - Seed data for user
            int userId = 1;
            string userName = "blade", email = "sneeps47@example.com", password = "password1";
            await SeedDataAsync(userId, userName, email, password);

            // Seed a new user
            UserInfo? newUser = new UserInfo
            {
                Id = 78,
                Username = "NewUser",
                Email = "newuser@example.com", // Unique email
                PasswordHash = password,
                FirstName = "Wesley",
                LastName = "Sneeps",
                Roles = new[] { "Admin" },


                Token = "Another-Dummy-Token",
                TokenExpiresAt = DateTime.UtcNow.AddHours(4)
            };

            // Act
            await _userService.InsertUserAsync(newUser);

            // Assert
            UserInfo? insertedEmail = await _dbContext.UserInfo.FindAsync(newUser.Id);
            insertedEmail.Should().NotBeNull();
            insertedEmail?.Username.Should().Be(newUser.Username);
            insertedEmail?.Email.Should().Be(newUser.Email);
            insertedEmail?.PasswordHash.Should().Be(newUser.PasswordHash);
            insertedEmail?.FirstName.Should().Be(newUser.FirstName);
            insertedEmail?.LastName.Should().Be(newUser.LastName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.InsertUserAsync"/> throws a <see cref="NotFoundException"/> when the user is null.
        /// </summary>
        [Fact]
        public async Task InsertUserAsync_ThrowsNotFoundException_WhenUserIsNullInMemory()
        {
            // Act & Assert
            Func<Task> act = async () => await _userService.InsertUserAsync(null!);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User is empty or null!");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.InsertUserAsync"/> throws an <see cref="InvalidOperationException"/> when the email is duplicate.
        /// </summary>
        [Fact]             
        public async Task InsertUserAsync_ThrowsInvalidOperationException_WhenEmailIsDuplicateInMemory()
        {
            // Arrange - Seed a user with an existing email
            string duplicateEmail = "duplicate@example.com";
            await SeedDataAsync(userId: 1, userName: "OriginalUser", email: duplicateEmail, password: "password123");

            // Seed a new user
            UserInfo duplicateUser = new UserInfo
            {
                Id = 2,
                Username = "DuplicateUser",
                Email = duplicateEmail, // Same email as seeded user
                PasswordHash = "password456",
                FirstName = "Duplicate",
                LastName = "Doe",
                Roles = Array.Empty<string>(), // Ensure Roles is initialized to avoid null

                Token = "Dummy-Token2",
                TokenExpiresAt = DateTime.UtcNow
            };

            // Act & Assert - Expect a DbUpdateException due to duplicate email
            Func<Task> act = async () => await _userService.InsertUserAsync(duplicateUser);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this email already exists.");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.InsertUserAsync"/> throws an <see cref="InvalidOperationException"/> when the username is duplicate.
        /// </summary>
        [Fact]
        public async Task InsertUserAsync_ThrowsInvalidOperationException_WhenUsernameIsDuplicateInMemory()
        {
            // Arrange
            string duplicateUsername = "ExistingUser";
            await SeedDataAsync(userId: 1, userName: duplicateUsername, email: "existing@example.com", password: "password123");

            UserInfo duplicateUser = new UserInfo
            {
                Id = 2,
                Username = duplicateUsername,
                Email = "new@example.com",
                PasswordHash = "password456",
                FirstName = "Duplicate",
                LastName = "Doe",
                Roles = Array.Empty<string>(),

                Token = "Dummy-Token3",
                TokenExpiresAt = DateTime.UtcNow
            };

            // Act & Assert
            Func<Task> act = async () => await _userService.InsertUserAsync(duplicateUser);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this username already exists.");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserAsync"/> successfully updates an existing user's data.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_SuccessfullyUpdatesExistingUserInMemory()
        {
            // Arrange
            int userId = 1;
            string originalUsername = "OriginalUser";
            string updatedUsername = "UpdatedUser";
            string email = "originaluser@example.com";
            string password = "originalpassword";

            // Seed the original user
            await SeedDataAsync(userId, originalUsername, email, password);

            // Retrieve the user to update
            UserInfo? userToUpdate = await _dbContext.UserInfo.FindAsync(userId);
            userToUpdate.Should().NotBeNull(); // Ensure the user exists

            // Update the user properties
            userToUpdate!.Username = updatedUsername;

            // Act
            await _userService.UpdateUserAsync(userToUpdate);

            // Assert
            UserInfo? updatedUser = await _dbContext.UserInfo.FindAsync(userId);
            updatedUser.Should().NotBeNull();
            updatedUser?.Username.Should().Be(updatedUsername); // Ensure the username was updated
            updatedUser?.Email.Should().Be(email); // Ensure other properties remain unchanged
            updatedUser?.PasswordHash.Should().Be(password); // Ensure password remains unchanged
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserAsync"/> throws a <see cref="NotFoundException"/> when attempting to update a non-existent user.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_ThrowsNotFoundException_WhenUserDoesNotExistInMemory()
        {
            // Arrange
            UserInfo nonExistentUser = new UserInfo
            {
                Id = 999, // An ID that doesn't exist
                Username = "NonExistentUser",
                Email = "nonexistent@example.com",
                PasswordHash = "password"
            };

            // Act
            Func<Task> act = () => _userService.UpdateUserAsync(nonExistentUser);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Id(User) not found!");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserAsync"/> throws an <see cref="InvalidOperationException"/> if an update is attempted with an email belonging to another user.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_ThrowsException_WhenEmailBelongsToAnotherUserInMemory()
        {
            // Arrange
            int userId1 = 1, userId2 = 2;
            string email1 = "user1@example.com", email2 = "user2@example.com";
            string username1 = "UserOne", username2 = "UserTwo";

            // Seed two users
            await SeedDataAsync(userId1, username1, email1, "password1");
            await SeedDataAsync(userId2, username2, email2, "password2");

            // Attempt to update User 1 with User 2's email
            UserInfo? userToUpdate = await _dbContext.UserInfo.FindAsync(userId1);
            userToUpdate.Should().NotBeNull(); // Ensure the user exists
            userToUpdate!.Email = email2; // Use User 2's email

            // Act
            Func<Task> act = async () => await _userService.UpdateUserAsync(userToUpdate);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this email already exists.");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserAsync"/> throws an <see cref="InvalidOperationException"/> if an update is attempted with a username belonging to another user.
        /// </summary>
        [Fact]
        public async Task UpdateUserAsync_ThrowsException_WhenUsernameBelongsToAnotherUserInMemory()
        {
            // Arrange
            int userId1 = 1, userId2 = 2;
            string email1 = "user1@example.com", email2 = "user2@example.com";
            string username1 = "UserOne", username2 = "UserTwo";

            // Seed two users
            await SeedDataAsync(userId1, username1, email1, "password1");
            await SeedDataAsync(userId2, username2, email2, "password2");

            // Attempt to update User 1 with User 2's username
            UserInfo? userToUpdate = await _dbContext.UserInfo.FindAsync(userId1);
            userToUpdate.Should().NotBeNull(); // Ensure the user exists
            userToUpdate!.Username = username2; // Use User 2's username

            // Act
            Func<Task> act = async () => await _userService.UpdateUserAsync(userToUpdate);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this username already exists.");
        }

        /// <summary>
        /// Verifies that <see cref="UserService.DeleteUserAsync"/> successfully deletes an existing user from the database.
        /// </summary>
        [Fact]
        public async Task DeleteUserAsync_SuccessfullyDeletesUserInMemory()
        {
            // Arrange
            int userId = 1;
            string username = "UserToDelete";
            string email = "deleteuser@example.com";
            string password = "password123";

            // Seed the user to delete
            await SeedDataAsync(userId, username, email, password);

            // Act
            await _userService.DeleteUserAsync(userId);

            // Assert
            UserInfo? deletedUser = await _dbContext.UserInfo.FindAsync(userId);
            deletedUser.Should().BeNull(); // Ensure the user was deleted
        }

        /// <summary>
        /// Verifies that <see cref="UserService.DeleteUserAsync"/> throws a <see cref="NotFoundException"/> when attempting to delete a non-existent user.
        /// </summary>
        [Fact]
        public async Task DeleteUserAsync_ThrowsNotFoundException_WhenUserDoesNotExistInMemory()
        {
            // Arrange
            int nonExistentUserId = 999;

            // Act
            Func<Task> act = () => _userService.DeleteUserAsync(nonExistentUserId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found!");
        }

        // ===========
        /// <summary>
        /// Seeds the database with test data for users to ensure controlled conditions in unit tests.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="userName">User name.</param>
        /// <param name="email">User email.</param>
        /// <param name="password">User password hash.</param>
        private async Task SeedDataAsync(int userId, string userName, string email, string password)
        {
            // Add or update the user in case it already exists to prevent conflicts
            UserInfo? user = new UserInfo
            {
                Id = userId,
                Username = userName,
                Email = email,
                PasswordHash = password,
                FirstName = "John",
                LastName = "Rambo",
                Roles = new[] { "Admin" }, // service-level tests won’t need authorization checks

                Token = "Dummy-Token",
                TokenExpiresAt = DateTime.UtcNow
            };

            await _dbContext.UserInfo.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            DetachAllEntities(_dbContext);
        }

        /// <summary>
        /// Detaches all tracked entities from the context to avoid state conflicts in subsequent operations.
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

