using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a user with essential user details 
    /// such as username, first and last name, and email address.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user record.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// This field is required and limited to a maximum of 75 characters.
        /// </summary>
        [Required, MaxLength(75, ErrorMessage = "Username is too long. Cannot be longer than 75 characters.")]
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// This field is required and limited to a maximum of 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "First Name is too long. Cannot be longer than 50 characters.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// This field is required and limited to a maximum of 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "Last Name is too long. Cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets the full name of the user by combining their first and last names. 
        /// This property is computed dynamically and is not stored in the database. 
        /// Returns an empty string if either first or last name is null.
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Gets or sets the email address of the user.
        /// This field is required, must be a valid email format, and serves as a primary contact identifier.
        /// </summary>
        [Required, EmailAddress]
        public string? Email { get; set; }
    }
}

