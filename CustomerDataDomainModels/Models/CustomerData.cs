using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerDataDomainModels.Models
{
    /// <summary>
    /// Represents customer information, including personal details, contact information, 
    /// date of birth, and a list of emails associated with the customer.
    /// </summary>
    public class CustomerData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// Serves as the primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; } // Primary key

        /// <summary>
        /// Gets or sets the customer's first name. This field is required and cannot exceed 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "First Name is too long. Cannot be longer than 50 characters.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name. This field is required and cannot exceed 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "Last Name is too long. Cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Combines the customer's first and last name to display a full name.
        /// This is a calculated property and is not mapped to the database.
        /// </summary>
        [NotMapped]
        public string? FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Gets or sets the customer's email address. This field is required and follows email format validation.
        /// </summary>
        [Required, EmailAddress]
        public string? Email { get; set; } // Make sure this matches the recipient emails

        /// <summary>
        /// Gets or sets the customer's phone number in the format (123) 456-7890. This field is required 
        /// and includes a regular expression for validation.
        /// </summary>
        [Required]
        [RegularExpression(@"^\([\d]{3}\) [\d]{3}-[\d]{4}$", ErrorMessage = "Phone number must be in the format (123) 456-7890")]
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's date of birth. This field is required 
        /// and displays the date in MM/dd/yyyy format.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")] // DisplayFormat works only with EditorFor and DisplayFor helpers;
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Navigation property representing the list of emails associated with the customer.
        /// Establishes a one-to-many relationship with the SendsEmails entity.
        /// Relationship with EmailsSent (One Customer can receive many emails)
        /// (Optional relation with EmailsSent)
        /// </summary>
        public ICollection<SendsEmails> EmailsSent { get; set; } = new List<SendsEmails>();
    }
}

