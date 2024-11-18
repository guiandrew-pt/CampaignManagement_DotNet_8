using System.ComponentModel.DataAnnotations;
using CustomerDataDTOs.Validation;

namespace CustomerDataDTOs.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a campaign, including essential campaign details,
    /// the user who created the campaign, and a collection of associated emails.
    /// </summary>
    public class CampaignDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the campaign.
        /// Serves as the primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the campaign.
        /// This field is required and limited to 50 characters.
        /// </summary>
        [Required, MaxLength(50, ErrorMessage = "The campaign name is too long. Cannot be longer than 50 characters.")]
        public string? CampaignName { get; set; }

        /// <summary>
        /// Gets or sets the campaign description.
        /// This field is required and limited to 150 characters.
        /// </summary>
        [Required, MaxLength(150, ErrorMessage = "The description is too long. Cannot be longer than 150 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the start date for the campaign.
        /// This field is required and displayed in MM/dd/yyyy format.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Range(typeof(DateTime), "01/01/2024", "01/01/2099", ErrorMessage = "The date must be between {1:MM/dd/yyyy} and {2:MM/dd/yyyy}.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the campaign.
        /// This field is required and displayed in MM/dd/yyyy format.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Range(typeof(DateTime), "01/01/2024", "01/01/2099", ErrorMessage = "The date must be between {1:MM/dd/yyyy} and {2:MM/dd/yyyy}.")]
        [DateRange("StartDate")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the campaign is active.
        /// This field is required and represents the current status of the campaign.
        /// </summary>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the person who created the campaign.
        /// This field is required and establishes a relationship with the user entity.
        /// </summary>
        // [Required]
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets limited information about the user who created the campaign.
        /// This property is used to display the user’s details in response data and is excluded from JSON serialization.
        /// </summary>
        // For returning the full user details when reading campaigns
        // For creation or updates when limited user details are sufficient
        public LimitedUserDto? CreatedByUser { get; set; }

        /// <summary>
        /// Gets or sets the list of emails associated with the campaign.
        /// Represents a one-to-many relationship where each campaign may have multiple emails sent under it.
        /// </summary>
        public List<SendsEmailsDto> EmailsSent { get; set; } = new List<SendsEmailsDto>();
    }
}

