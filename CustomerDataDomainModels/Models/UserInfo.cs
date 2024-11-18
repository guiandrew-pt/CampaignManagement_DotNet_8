using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerDataDomainModels.Models
{
    /// <summary>
    /// Represents a user with personal information, authentication details, and associated roles.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// Serves as the primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username for the user. This field is required and limited 
        /// to a maximum of 75 characters.
        /// </summary>
        [Required, MaxLength(75, ErrorMessage = "Username is too long. Cannot be longer than 75 characters.")]
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the user's email address. This field is required and
        /// must follow valid email format.
        /// </summary>
        [Required, EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user's first name. This field is required and limited 
        /// to a maximum of 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "First Name is too long. Cannot be longer than 50 characters.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name. This field is required and limited 
        /// to a maximum of 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "Last Name is too long. Cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the user. This field is required and can include multiple roles,
        /// such as "Admin" or "User".
        /// </summary>
        [Required]
        public string[]? Roles { get; set; } // Array of roles, such as ["Admin", "User"]
        // public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the hashed password for the user. This field is required 
        /// for security purposes and is used in password validation.
        /// </summary>
        [Required]
        public string? PasswordHash { get; set; } // Hashed password for security

        /// <summary>
        /// Gets or sets the JWT token assigned to the user for authentication purposes.
        /// This field is required.
        /// </summary>
        [Required]
        public string? Token { get; set; } // JWT Token

        /// <summary>
        /// Gets or sets the expiration date and time for the user's token.
        /// Used to determine if the token is still valid or has expired.
        /// </summary>
        public DateTime TokenExpiresAt { get; set; } // Optional: Expiration time of the token

        /// <summary>
        /// Indicates whether the user's token has been revoked. When true, the token
        /// is considered invalid for future authentication attempts.
        /// This property is only available in the API for security reasons.
        /// This property is primarily used for security purposes and is exposed only via the API.
        /// </summary>
        // [JsonProperty("IsTokenRevoked")]
        public bool IsTokenRevoked { get; set; } // Optional: set to true when the user logs out

        /// <summary>
        /// Records the last activity timestamp of the user. 
        /// This value is used to implement timeout functionality for users who do not explicitly log out.
        /// </summary>
        public DateTime LastActive { get; set; } // To implement the time out if user do not logout.

        /// <summary>
        /// Gets or sets the collection of campaigns created by the user.
        /// Represents a one-to-many relationship where one user can create multiple campaigns.
        /// </summary>
        [JsonIgnore]
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}

