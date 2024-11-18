using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing minimal user details, used for association with other entities.
    /// </summary>
    public class LimitedUserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user record.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
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
        /// </summary>
        public string? Email { get; set; }
    }
}

