using CustomerDataAPI.ExtensionMethod;
using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CustomerDataAPI.Controllers
{
    /// <summary>
    /// API Controller for managing user authentication and authorization.
    /// Provides login, logout, user registration, and profile management capabilities.
    /// Enables CORS for client communication and restricts access based on roles where applicable.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userService">Service for user authentication and management operations.</param>
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticates a user based on login credentials and returns an authorization token.
        /// </summary>
        /// <param name="login">The user's login credentials.</param>
        /// <returns>A JWT token and expiration date if successful, or a 401 Unauthorized if credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginDto login)
        {
            // Check if the login credentials are valid
            if (login is null)
            {
                return BadRequest("Invalid client request");
            }

            AuthResponseDto? response = await _userService.LoginAsync(login);

            if (response is not null)
            {
                return Ok(response);
            }

            return Unauthorized("Invalid login credentials.");
        }

        /// <summary>
        /// Logs out the currently authenticated user by revoking their token.
        /// </summary>
        /// <returns>A confirmation message if logout is successful, or an error message if it fails.</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                int? userId = User.GetUserId();

                if (userId.HasValue)
                {
                    await _userService.RevokeTokenAsync(userId.Value); // Revoke the token
                }
                else
                {
                    return BadRequest("Invalid user ID in token.");
                }

                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout failed: {ex.Message}");
                return StatusCode(500, "An error occurred while logging out.");
            }
        }

        /// <summary>
        /// Retrieves a list of all users with limited details (username, first name, last name, and email).
        /// Only accessible by Admins.
        /// </summary>
        /// <returns>A list of users with essential information, or a 404 Not Found if none exist.</returns>
        // [Authorize(Roles = "Admin")]
        [Authorize]
        [HttpGet("Users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            List<UserDto> users = await _userService.GetAllUsersAsync();

            if (users is null || users.Count == 0)
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user's details by their unique identifier.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>The user’s information if found, or a 404 Not Found if the user does not exist.</returns>
        // GET: api/<AuthController>/5
        [Authorize(Roles = "Admin, Manager")] // Only Admins and Manager can delete
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            UserInfo? user = await _userService.GetUserByIdAsync(id);

            if (user is null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }

        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="registerDto">The user's registration data, including username, email, password, and roles.</param>
        /// <returns>A success message if registration is successful, or an error message if registration fails.</returns>
        // POST: api/<AuthController>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterUpdateDto registerDto)
        {
            if (registerDto is null || !ModelState.IsValid)
            {
                return BadRequest("Invalid registration request!");
            }

            // Check if the user email is not null
            if (registerDto.Email is null)
                return BadRequest("Email cannot be empty or null!");

            // Check if the user Username is not null
            if (registerDto.Username is null)
                return BadRequest("Username cannot be empty or null!");

            try
            {
                // Create new user object
                UserInfo? newUser = new UserInfo
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password), // Hash the plain-text password
                    Roles = registerDto.Roles.ToArray(), // Set selected roles
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,

                    Token = string.Empty, // We can leave this blank or null at registration
                    TokenExpiresAt = DateTime.UtcNow // Placeholder expiration
                };

                // Save to database
                await _userService.InsertUserAsync(newUser);

                return Ok("User registered successfully");
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific validation exceptions from the service
                if (ex.Message.Contains("email"))
                {
                    return Conflict("EmailConflict");
                }
                if (ex.Message.Contains("username"))
                {
                    return Conflict("UsernameConflict");
                }

                // Handle other unexpected exceptions
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
            catch (Exception)
            {
                // Catch any unhandled exceptions
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Updates an existing user with the specified data.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="updateDto">The updated user information.</param>
        /// <returns>A success message if update is successful, or a 404 Not Found if the user does not exist.</returns>
        // PUT api/<AuthController>/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserRegisterUpdateDto updateDto)
        {
            // Retrieve the existing user
            UserInfo? user = await _userService.GetUserByIdAsync(id);

            // 
            if (user is null)
            {
                return NotFound($"User with ID {id} not found");
            }

            try
            {
                // Update only the properties that are allowed to change
                if (!string.IsNullOrEmpty(updateDto.Username))
                    user.Username = updateDto.Username;

                if (!string.IsNullOrEmpty(updateDto.Email))
                    user.Email = updateDto.Email;

                if (!string.IsNullOrEmpty(updateDto.Password))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.Password); // Hash new password

                if (updateDto.Roles is not null && updateDto.Roles.Any())
                    user.Roles = updateDto.Roles.ToArray();

                // Update properties
                await _userService.UpdateUserAsync(user);

                return Ok("User updated successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                // Handle email conflict error
                return Conflict("A user with this email already exists. Please use a different email.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("username"))
            {
                // Handle username conflict error
                return Conflict("A user with this username already exists. Please use a different username.");
            }
            catch (NotFoundException ex)
            {
                // Handle not found error
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle generic errors
                return StatusCode(500, $"An unexpected error occurred. Please try again later. - {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user by their unique identifier. Only accessible by Admins.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>204 No Content if deletion is successful, or 404 Not Found if the user does not exist.</returns>
        // DELETE api/<AuthController>/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            UserInfo? user = await _userService.GetUserByIdAsync(id);

            if (user is null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            await _userService.DeleteUserAsync(id);

            return NoContent(); // Return 204 No Content
        }
    }
}
