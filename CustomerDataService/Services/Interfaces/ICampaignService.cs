using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;

namespace CustomerDataService.Services.Interfaces
{
    /// <summary>
    /// Interface for managing campaign-related operations, including retrieving, creating, updating, and deleting campaigns.
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>
        /// Retrieves a list of all campaigns, including the associated emails for each campaign.
        /// </summary>
        /// <returns>A list of <see cref="Campaign"/> objects representing all campaigns.</returns>
        public Task<List<CampaignDto>> FindAllAsync();

        /// <summary>
        /// Retrieves a specific campaign by its unique identifier, including associated emails.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign.</param>
        /// <returns>The <see cref="CampaignDto"/> object if found; otherwise, throws an exception if not found.</returns>
        public Task<CampaignDto> FindByIdAsync(int id);

        /// <summary>
        /// Asynchronously inserts a new campaign record into the database, associating it with the specified user.
        /// </summary>
        /// <param name="campaign">The <see cref="Campaign"/> object containing the data for the campaign to insert.</param>
        /// <param name="userId">The ID of the user creating the campaign, which will be associated with this campaign record.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task InsertAsync(Campaign campaign, int userId);

        /// <summary>
        /// Updates an existing campaign record in the database.
        /// </summary>
        /// <param name="campaign">The <see cref="Campaign"/> object with updated campaign information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateAsync(Campaign campaign);

        /// <summary>
        /// Deletes a campaign record based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RemoveAsync(int id);
    }
}

