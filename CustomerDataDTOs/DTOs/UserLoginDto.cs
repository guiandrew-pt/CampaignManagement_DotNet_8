using System.ComponentModel.DataAnnotations;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object for user login. 
    /// Represents the data required to authenticate a user, including their username or email and password.
    /// </summary>
    public class UserLoginDto
    {
        /// <summary>
        /// Gets or sets the user's username or email address for login.
        /// This field is required, and an error message is shown if it is not provided.
        /// </summary>
        [Required(ErrorMessage = "Username or Email is required")]
        public string? UsernameOrEmail { get; set; }

        /// <summary>
        /// Gets or sets the user's password for login.
        /// This field is required to authenticate the user.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
}

