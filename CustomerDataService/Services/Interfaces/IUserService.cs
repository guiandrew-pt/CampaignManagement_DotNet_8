using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;

namespace CustomerDataService.Services.Interfaces
{
    /// <summary>
    /// Interface for user management services, providing methods for user authentication, 
    /// retrieval, insertion, update, and deletion operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user based on login credentials and returns an authentication response.
        /// </summary>
        /// <param name="login">The login credentials, including username/email and password.</param>
        /// <returns>
        /// An <see cref="AuthResponseDto"/> containing the JWT token, user roles, and other 
        /// authentication details upon successful login.
        /// </returns>
        Task<AuthResponseDto> LoginAsync(UserLoginDto login);

        /// <summary>
        /// Revokes an active authentication token, effectively logging out the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to log out.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RevokeTokenAsync(int userId);

        /// <summary>
        /// Retrieves a list of all users in the system, returning basic user information 
        /// such as id, username, first name, last name, and email. 
        /// The method is intended for administrative purposes and does not include sensitive details.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="UserDto"/> 
        /// containing information about each user.</returns>
        Task<List<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves user information by user ID.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="UserInfo"/> object containing the user's details if found.
        /// </returns>
        Task<UserInfo> GetUserByIdAsync(int id);

        /// <summary>
        /// Retrieves user information based on their email address.
        /// </summary>
        /// <param name="email">The email address associated with the user.</param>
        /// <returns>
        /// A <see cref="UserInfo"/> object containing the user's details if found; 
        /// otherwise, <c>null</c>.
        /// </returns>
        // Task<UserInfo?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Inserts a new user record into the database.
        /// </summary>
        /// <param name="user">The user information to insert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertUserAsync(UserInfo user);

        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="user">The user object with updated details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateUserAsync(UserInfo user);

        /// <summary>
        /// Deletes a user record based on user ID.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteUserAsync(int id);
    }
}

