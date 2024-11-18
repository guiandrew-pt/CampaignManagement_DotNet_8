using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomerDataEnum.Enum;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object representing an email that was sent as part of a campaign.
    /// Contains information about the recipient, email content, sent date, status, and related campaign and customer details.
    /// </summary>
    public class SendsEmailsDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sent email record.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the recipient.
        /// This field is required and must be a valid email format.
        /// </summary>
        [Required, EmailAddress]
        public string? RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// This field is required, with a maximum length of 30 characters.
        /// </summary>
        [Required, MaxLength(30, ErrorMessage = "Subject is too long. Cannot be longer than 30 characters.")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the main content of the email.
        /// This field is required.
        /// </summary>
        [Required]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the date the email was sent.
        /// This field is required and is formatted as "MM/dd/yyyy".
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Range(typeof(DateTime), "01/01/1901", "01/01/2099", ErrorMessage = "The date must be between {1:MM/dd/yyyy} and {2:MM/dd/yyyy}.")]
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the email (e.g., Sent, Failed, Delivered).
        /// This field is required.
        /// </summary>
        [Required]
        public Status? Status { get; set; }

        /// <summary>
        /// Gets or sets the ID of the campaign that will be sent in this email.
        /// This field is required.
        /// </summary>
        [Required]
        public int CampaignId { get; set; }

        /// <summary>
        /// Gets or sets the name of the campaign that will be in email.
        /// This field is optional. (nullable)
        /// </summary>
        public string? CampaignName { get; set; } // Name of the Campaign (nullable)

        /// <summary>
        /// Gets or sets the ID of the customer associated with this email, if applicable.
        /// This field is optional. (nullable)
        /// </summary>
        public int? CustomerDataId { get; set; }

        /// <summary>
        /// Gets or sets the name of the customer associated with this email, if applicable.
        /// This field is optional. (nullable)
        /// </summary>
        public string? CustomerDataName { get; set; }

        /// <summary>
        /// Gets or sets the username of the user who created the associated campaign, if applicable.
        /// This field is optional and may be null if no user information is available.
        /// </summary>
        public string? CreatedByUsername { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user who created the associated campaign, if applicable.
        /// This field is optional and may be null if no user information is available.
        /// </summary>
        public string? CreatedByFirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user who created the associated campaign, if applicable.
        /// This field is optional and may be null if no user information is available.
        /// </summary>
        public string? CreatedByLastName { get; set; }

        /// <summary>
        /// Gets the full name of the user who created the associated campaign by combining 
        /// their first and last names. This field is not stored in the database and 
        /// will return an empty string if no user information is available.
        /// </summary>
        [NotMapped]
        public string FullName => $"{CreatedByFirstName} {CreatedByLastName}";

        /// <summary>
        /// Gets or sets the email address of the user who created the associated campaign, if applicable.
        /// This field is optional and may be null if no user information is available.
        /// </summary>
        public string? CreatedByEmail { get; set; }
    }
}

