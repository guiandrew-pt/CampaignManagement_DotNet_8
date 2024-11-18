using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) used for registering and updating user information.
    /// Contains properties for user identity details, contact information, roles, and password.
    /// </summary>
    public class UserRegisterUpdateDto
    {
        /// <summary>
        /// Gets or sets the username for the user.
        /// This field is required and has a maximum length of 75 characters.
        /// </summary>
        [Required, MaxLength(75)]
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// This field is required and must be a valid email format.
        /// </summary>
        [Required, EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// This field is required and cannot exceed 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "First Name is too long. Cannot be longer than 50 characters.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// This field is required and cannot exceed 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "Last Name is too long. Cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Combines the user's first and last names into a single full name.
        /// This property is not mapped to the database.
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Gets or sets the user's password.
        /// This field is required and is used for authentication.
        /// </summary>
        [Required]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned to the user.
        /// This property is serialized as "Roles" in JSON format.
        /// </summary>
        [JsonProperty("Roles")]
        // public string[]? Roles { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}

