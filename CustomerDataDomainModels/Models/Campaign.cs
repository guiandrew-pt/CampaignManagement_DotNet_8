using System.ComponentModel.DataAnnotations;

namespace CustomerDataDomainModels.Models
{
    /// <summary>
    /// Represents a campaign within the application, containing details 
    /// like campaign name, description, start and end dates, status, creator, and related emails sent as part of the campaign.
    /// </summary>
    public class Campaign
    {
        /// <summary>
        /// Gets or sets the unique identifier for the campaign.
        /// Serves as the primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the campaign. This field is required and cannot exceed 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "The campaign name is too long. Cannot be longer than 50 characters.")]
        public string? CampaignName { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the campaign. This field is required and cannot exceed 150 characters.
        /// </summary>
        [Required, MaxLength(150, ErrorMessage = "The description is too long. Cannot be longer than 150 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the start date of the campaign. This field is required and is formatted as MM/dd/yyyy.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")] // DisplayFormat works only with EditorFor and DisplayFor helpers;
        public DateTime StartDate { get; set; } // Campaign start date

        /// <summary>
        /// Gets or sets the end date of the campaign. This field is required, formatted as MM/dd/yyyy,
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")] // DisplayFormat works only with EditorFor and DisplayFor helpers;
        public DateTime EndDate { get; set; } // Campaign end date

        /// <summary>
        /// Gets or sets the status of the campaign. A value of true indicates the campaign is active,
        /// while false indicates it is inactive. This field is required.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } // Status of the campaign (active or inactive)

        /// <summary>
        /// Gets or sets the ID of the user who created the campaign. Acts as a foreign key to the UserInfo entity.
        /// </summary>
        // [Required]
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Navigation property representing the user who created the campaign.
        /// </summary>
        // [JsonIgnore]
        public virtual UserInfo? CreatedByUser { get; set; }

        /// <summary>
        /// Navigation property representing the list of emails sent as part of this campaign.
        /// Navigation property (one-to-many relationship with SendsEmails)
        /// Navigation property for emails sent as part of this campaign
        /// Establishes a one-to-many relationship with the SendsEmails entity.
        /// </summary>
        public ICollection<SendsEmails> EmailsSent { get; set; } = new List<SendsEmails>();
    }
}

