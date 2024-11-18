using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Data;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomerDataService.Services
{
    /// <summary>
    /// Service for handling user-related operations, including login, registration, and token management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly CustomerDataContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/> with the specified data context.
        /// Constructor to inject the database context for data access.
        /// </summary>
        /// <param name="customerDataContext">The data context for accessing user information.</param>
        public UserService(CustomerDataContext customerDataContext)
        {
            _context = customerDataContext;
        }

        /// <summary>
        /// Handles user login, generates a JWT token, and resets the token revocation flag upon successful login.
        /// </summary>
        /// <param name="login">The user login data containing credentials.</param>
        /// <returns>
        /// An <see cref="AuthResponseDto"/> containing the generated JWT token and its expiration time.
        /// </returns>
        /// <exception cref="NotFoundException">Thrown if the user cannot be found or authentication fails.</exception>
        public async Task<AuthResponseDto> LoginAsync(UserLoginDto login)
        {
            // Validate that login data is provided
            if (login is null)
                throw new NotFoundException("User not found");

            // Validate the user credentials (we can use Identity or custom validation here)
            // Authenticate user credentials
            UserInfo? user = await AuthenticateUserAsync(login);
            if (user is null)
                throw new NotFoundException("User is null or not found."); // Throw exception if user authentication fails

            // Automatic session expiry check
            if (!IsUserActive(user))
            {
                // If session is inactive, perform automatic "soft logout" to clear expired session data
                user.IsTokenRevoked = true;
                user.LastActive = DateTime.MinValue;
                await UpdateUserAsync(user);  // Persist session cleanup to database
            }

            //// Check if user is already logged in and active
            //if (await IsUserLoggedInAsync(user.Id))
            //    throw new InvalidOperationException("User already logged in. Please log out first.");

            // Generate JWT and update user's session data
            string? tokenString = GenerateJSONWebToken(user); // Generate JWT with claims

            // Reset the token revocation flag on successful login.
            user.IsTokenRevoked = false;
            user.LastActive = DateTime.UtcNow; // Update LastActive on login
            await UpdateUserAsync(user);

            // Return authentication response with token and expiration time
            return new AuthResponseDto
            {
                Token = tokenString,
                Expires = user.TokenExpiresAt
            };
        }

        /// <summary>
        /// Revokes the JWT token for a specified user by setting the token expiration to the past and marking it as revoked.
        /// Revokes the JWT token for a specified user, effectively logging the user out by 
        /// setting the token expiration to the past and marking it as revoked in the system.
        /// </summary>
        /// <param name="userId">The ID of the user whose token will be revoked.</param>
        /// <exception cref="Exception">Thrown if the user is not found.</exception>
        public async Task RevokeTokenAsync(int userId)
        {
            // Retrieve the user by ID
            UserInfo? user = await GetUserByIdAsync(userId); // Fetch the user from the database

            if (user is null)
                throw new Exception("User not found");

            user.TokenExpiresAt = DateTime.UtcNow; // Expire token immediately
            user.IsTokenRevoked = true; // Mark token as revoked
            user.LastActive = DateTime.MinValue; // Optionally set LastActive to a default (inactive) value

            await UpdateUserAsync(user); // Save changes to the database
        }

        /// <summary>
        /// Retrieves a list of all users in the system, containing non-sensitive information such as ID, 
        /// username, first name, last name, and email. 
        /// This method is intended for administrative purposes and excludes sensitive details.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, which returns a list of <see cref="UserDto"/> 
        /// objects containing basic information about each user in the system.</returns>
        /// <exception cref="Exception">Thrown if no users are found in the database.</exception>
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            List<UserDto>? users = await _context.UserInfo
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                })
                .ToListAsync();

            if (users is null || !users.Any())
                throw new Exception("User not found");

            return users;
        }

        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        /// <param name="id">The unique ID of the user to retrieve.</param>
        /// <returns>The <see cref="UserInfo"/> of the requested user.</returns>
        /// <exception cref="NotFoundException">Thrown if the user cannot be found.</exception>
        public async Task<UserInfo> GetUserByIdAsync(int id)
        {
            //return await _context.UserInfo
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(u => u.Id == id) ??
            //    throw new NotFoundException("User is empty or null!");

            UserInfo? user = await _context.UserInfo
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                throw new NotFoundException("User is empty or null!");

            user.LastActive = DateTime.UtcNow; // Update LastActive
            await UpdateUserAsync(user); // Save the updated LastActive timestamp

            return user;
        }

        /// <summary>
        /// Retrieves a user by their email address. (returns null if not found)
        /// </summary>
        /// <param name="email">The email of the user to retrieve.</param>
        /// <returns>The <see cref="UserInfo"/> of the requested user, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if the email is null.</exception>
        //public async Task<UserInfo> GetUserByEmailAsync(string email)
        //{
        //    if (email is null)
        //        throw new NotFoundException("User is null!");

        //    return await _context.UserInfo
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(u => u.Email == email) ??
        //        throw new NotFoundException("User not found!");
        //}

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user information to add.</param>
        /// <exception cref="NotFoundException">Thrown if the user information is null.</exception>
        public async Task InsertUserAsync(UserInfo user)
        {
            if (user is null)
                throw new NotFoundException("User is empty or null!");

            // Check if a user with the same email already exists
            UserInfo? existingEmailUser = await GetUserByEmailAsync(user.Email!);
            if (existingEmailUser is not null)
                throw new InvalidOperationException("A user with this email already exists.");

            // Check if a user with the same username already exists
            UserInfo? existingUsernameUser = await GetUserByUsernameAsync(user.Username!);
            if (existingUsernameUser is not null)
                throw new InvalidOperationException("A user with this username already exists.");

            _context.Add(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing user's information in the database.
        /// </summary>
        /// <param name="user">The updated user information.</param>
        /// <exception cref="NotFoundException">Thrown if the user does not exist in the database.</exception>
        /// <exception cref="DbConcurrencyException">Thrown if a concurrency error occurs while updating.</exception>
        public async Task UpdateUserAsync(UserInfo user)
        {
            // Check if the user exists in the database
            bool userExists = await _context.UserInfo.AnyAsync(u => u.Id == user.Id);
            if (!userExists)
                throw new NotFoundException("Id(User) not found!");

            // Check if the email is used by another user
            bool emailExistsForAnotherUser = await _context.UserInfo
                .AnyAsync(u => u.Email == user.Email && u.Id != user.Id);
            if (emailExistsForAnotherUser)
                throw new InvalidOperationException("A user with this email already exists.");

            // Check if the username is used by another user
            bool usernameExistsForAnotherUser = await _context.UserInfo
                .AnyAsync(u => u.Username == user.Username && u.Id != user.Id);
            if (usernameExistsForAnotherUser)
                throw new InvalidOperationException("A user with this username already exists.");

            try
            {
                // Update the user
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DbConcurrencyException(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a user from the database based on their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user does not exist in the database.</exception>
        /// <exception cref="IntegrityException">Thrown if a database error occurs during deletion.</exception>
        public async Task DeleteUserAsync(int id)
        {
            // Find the user by ID or throw a NotFoundException if the user does not exist
            UserInfo? user = await _context.UserInfo.FindAsync(id)
                    ?? throw new NotFoundException("User not found!");

            try
            {
                // Attempt to remove the user and save changes
                _context.UserInfo.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Handle database errors, such as foreign key constraints
                throw new IntegrityException("Can't delete User!");
            }
        }

        /// <summary>
        /// Retrieves a user by their email address. (returns null if not found)
        /// </summary>
        /// <param name="email">The email of the user to retrieve.</param>
        /// <returns>The <see cref="UserInfo"/> of the requested user, or null if not found.</returns>
        private async Task<UserInfo?> GetUserByEmailAsync(string email)
        {
            if (email is null)
                throw new ArgumentException("Email cannot be null", nameof(email));

            return await _context.UserInfo
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Retrieves a user by their username. (returns null if not found)
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The <see cref="UserInfo"/> of the requested user, or null if not found.</returns>
        private async Task<UserInfo?> GetUserByUsernameAsync(string username)
        {
            if (username is null)
                throw new ArgumentException("Username cannot be null", nameof(username));

            return await _context.UserInfo
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user, including user roles and claims.
        /// Generates a JWT token for authenticated users.
        /// </summary>
        /// <param name="userInfo">The user information for generating the token.</param>
        /// <returns>The generated JWT token as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if userInfo is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if JWT settings are not configured.</exception>
        private string GenerateJSONWebToken(UserInfo userInfo)
        {
            if (userInfo is null)
                throw new ArgumentNullException(nameof(userInfo), "UserInfo cannot be null");

            // Load JWT settings from environment variables
            string? issuer = Env.GetString("JWT_ISSUER_DEV");
            string? audience = Env.GetString("JWT_AUDIENCE_DEV");
            string? secret = Env.GetString("JWT_SECRET");

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT settings are not properly configured!");
            }

            // Define security key and credentials for signing the token
            SymmetricSecurityKey? securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            SigningCredentials? credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //// Create claims based on the user information
            //List<Claim>? claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, userInfo?.Username ?? ""),
            //    new Claim(ClaimTypes.Email, userInfo?.Email ?? "")
            //};

            // Define claims based on user information
            List<Claim>? claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo?.Username ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, userInfo?.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID for the token
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued at time
                new Claim("id", userInfo!.Id.ToString()) // Add the user ID as a custom claim
            };

            // Add the user roles as claims (e.g., Admin, User)
            if (userInfo?.Roles is not null)
            {
                claims.AddRange(userInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            // Create the token
            JwtSecurityToken? token = new JwtSecurityToken(
                issuer,
                audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120), // Token expires in 2 hours
                signingCredentials: credentials);

            // Store token expiration time in user info
            userInfo!.TokenExpiresAt = token.ValidTo!;
            // Return the generated token
            return new JwtSecurityTokenHandler().WriteToken(token); // Return the serialized token
        }

        /// <summary>
        /// Authenticates the user credentials and returns user information if valid.
        /// </summary>
        /// <param name="login">The login credentials.</param>
        /// <returns>The authenticated <see cref="UserInfo"/> if successful, otherwise null.</returns>
        private async Task<UserInfo?> AuthenticateUserAsync(UserLoginDto login)
        {
            //// Validate the user's credentials (this is where you'd normally check the database)
            //// For demonstration, let's assume a single hardcoded user exists
            //if (login.UsernameOrEmail == "testuser" || login.UsernameOrEmail == "testuser@example.com" && login.Password == "password")
            //{
            //    return await Task.FromResult(new UserInfo
            //    {
            //        Id = 1,
            //        Username = "testuser",
            //        Email = "testuser@example.com",
            //        FirstName = "John",
            //        LastName = "Doe",
            //        Roles = new[] { "Admin", "User" } // Assigning both Admin and User roles
            //    });
            //}

            // return null; // Return null if authentication fails

            // Retrieve the user by matching the username or email in the database
            UserInfo? user = await _context.UserInfo
                .FirstOrDefaultAsync(u => (u.Username == login.UsernameOrEmail || u.Email == login.UsernameOrEmail));

            // If the user exists and the password matches the stored hash, return the user
            if (user is not null && BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return user; // Return user if credentials are valid
            }

            // Return null if authentication fails
            return null;
        }

        /// <summary>
        /// Determines whether a user is currently active based on their last activity timestamp.
        /// </summary>
        /// <param name="user">The user to check for activity.</param>
        /// <returns>
        /// <c>true</c> if the user was active within the defined timeout period; otherwise, <c>false</c>.
        /// </returns>
        private bool IsUserActive(UserInfo user)
        {
            // Define an activity timeout, e.g., 2 hours
            TimeSpan activityTimeout = TimeSpan.FromHours(2);

            // Determine if the user's last activity occurred within the timeout period
            // Check if the user was active within the timeout period
            bool isActive = DateTime.UtcNow - user.LastActive < activityTimeout;

            // Log the user's activity status for debugging purposes
            // Console.WriteLine($"User {user.Id} active status: {isActive}"); // Testing only
            return isActive;
        }
    }
}

