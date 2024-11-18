using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CustomerDataEnum.Enum;

namespace CustomerDataDomainModels.Models
{
    /// <summary>
    /// Represents an email sent as part of a campaign, including details about 
    /// the recipient, content, status, and associations with both a campaign and a customer.
    /// </summary>
    public class SendsEmails
    {
        /// <summary>
        /// Gets or sets the unique identifier for the email record.
        /// Serves as the primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the recipient's email address. This field is required and
        /// must adhere to valid email format.
        /// </summary>
        [Required, EmailAddress]
        public string? RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject line of the email. This field is required 
        /// and limited to a maximum of 30 characters.
        /// </summary>
        [Required, MaxLength(30, ErrorMessage = "Subject is too long. Cannot be longer than 30 characters.")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the main content or body of the email. This field is required.
        /// Content/body of the email
        /// </summary>
        [Required]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the date the email was sent. Displays in MM/dd/yyyy format.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")] // DisplayFormat works only with EditorFor and DisplayFor helpers;
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the email (e.g., Sent, Failed).
        /// </summary>
        [Required]
        public Status Status { get; set; }

        /// <summary>
        /// Gets or sets the ID of the associated campaign. Serves as a foreign key in the database.
        /// Foreign key relationship with Campaign (existing relation)
        /// </summary>
        [Required]
        public int CampaignId { get; set; }

        /// <summary>
        /// Navigation property linking to the associated campaign.
        /// Represents a many-to-one relationship with the Campaign entity.
        /// Navigation property (optional)(nullable)
        /// </summary>
        [JsonIgnore]
        public virtual Campaign? Campaign { get; set; } // EF will populate this based on CampaignId

        /// <summary>
        /// Gets or sets the ID of the associated customer, if any. Nullable foreign key.
        /// Optional relation to CustomerData // Optional foreign key to CustomerData (nullable)
        /// </summary>
        public int? CustomerDataId { get; set; }

        /// <summary>
        /// Navigation property linking to the associated customer, if applicable.
        /// Represents a many-to-one relationship with the CustomerData entity.
        /// Navigation property (optional)(nullable)
        /// </summary>
        [JsonIgnore]
        public virtual CustomerData? CustomerData { get; set; } // EF will populate this based on CustomerDataId
    }
}

