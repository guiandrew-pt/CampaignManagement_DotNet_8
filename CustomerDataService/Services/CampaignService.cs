using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Data;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerDataService.Services
{
    /// <summary>
    /// Service class for managing `Campaign` records, providing operations, offering functionality to 
    /// to retrieve, add, update, and delete campaigns, along with related email data.
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly CustomerDataContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="CampaignService"/> with the specified database context.
        /// </summary>
        /// <param name="customerDataContext">Database context for campaign and related data.</param>
        public CampaignService(CustomerDataContext customerDataContext)
        {
            _context = customerDataContext;
        }

        /// <summary>
        /// Retrieves all `Campaign` records, including associated `SendsEmails` and the user who created the campaign.
        /// </summary>
        /// <returns>A list of `CampaignDto` objects. Each entry contains campaign details, associated emails sent, 
        /// and basic information about the user who created the campaign.</returns>
        public async Task<List<CampaignDto>> FindAllAsync()
        {
            // Eager loading EmailsSent to include associated emails with each Campaign record.
            // return await _context.Campaign.Include(c => c.EmailsSent).ToListAsync();

            // Eagerly load associated EmailsSent with each Campaign record and only the specified fields from the related UserInfo.
            // Fetch campaigns and only the specified fields from the associated UserInfo
            return await _context.Campaign
                .Include(c => c.CreatedByUser)
                .Include(c => c.EmailsSent) // Include EmailsSent
                .Select(campaign => new CampaignDto
                {
                    Id = campaign.Id,
                    CampaignName = campaign.CampaignName,
                    Description = campaign.Description,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    IsActive = campaign.IsActive,
                    CreatedByUserId = campaign.CreatedByUserId,

                    // Load basic user information if the user exists (e.g., the user who created the campaign).
                    CreatedByUser = campaign.CreatedByUser != null ? new LimitedUserDto
                    {
                        Id = campaign.CreatedByUser.Id,
                        Username = campaign.CreatedByUser.Username,
                        FirstName = campaign.CreatedByUser.FirstName,
                        LastName = campaign.CreatedByUser.LastName,
                        Email = campaign.CreatedByUser.Email
                    } : null,

                    // Load associated emails (SendsEmails) for each campaign.
                    EmailsSent = campaign.EmailsSent.Select(email => new SendsEmailsDto
                    {
                        Id = email.Id,
                        RecipientEmail = email.RecipientEmail,
                        Subject = email.Subject,
                        Content = email.Content,
                        SentDate = email.SentDate,
                        Status = email.Status
                    }).ToList()
                }).ToListAsync();
        }

        /// <summary>
        /// Finds a specific `Campaign` record by its unique identifier, including associated emails and user information.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign to retrieve.</param>
        /// <returns>
        /// A `CampaignDto` containing campaign details, the user who created the campaign, 
        /// and a collection of associated emails if the campaign is found.
        /// Throws a `NotFoundException` if no campaign with the specified ID exists.
        /// </returns>
        /// <exception cref="NotFoundException">Thrown if no campaign with the specified ID is found.</exception>
        public async Task<CampaignDto> FindByIdAsync(int id)
        {
            //return await _context.Campaign
            //    .AsNoTracking() // Optimizes for read-only operations by not tracking changes.
            //    .Include(c => c.EmailsSent) // Eager loading of related `SendsEmails` entities
            //    .FirstOrDefaultAsync(c => c.Id == id) // Search for campaign by ID
            //    ?? throw new NotFoundException("Campaign not found!");

            // Attempt to retrieve the campaign record by ID, with associated emails and user details.
            CampaignDto? campaign = await _context.Campaign
                .AsNoTracking() // Optimizes for read-only operations by not tracking changes(to avoid tracking changes).
                .Include(c => c.EmailsSent) // Eager loading of related `SendsEmails` entities
                .Where(c => c.Id == id) // Filter by the specified campaign ID.
                .Select(c => new CampaignDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    CreatedByUserId = c.CreatedByUserId,

                    // Include basic user information if the user who created the campaign exists.
                    CreatedByUser = c.CreatedByUser != null ? new LimitedUserDto
                    {
                        Id = c.CreatedByUser.Id,
                        Username = c.CreatedByUser.Username,
                        FirstName = c.CreatedByUser.FirstName,
                        LastName = c.CreatedByUser.LastName,
                        Email = c.CreatedByUser.Email
                    } : null,

                    // Include a collection of associated emails sent in the campaign.
                    EmailsSent = c.EmailsSent.Select(email => new SendsEmailsDto
                    {
                        Id = email.Id,
                        RecipientEmail = email.RecipientEmail,
                        Subject = email.Subject,
                        Content = email.Content,
                        SentDate = email.SentDate,
                        Status = email.Status
                    }).ToList()
                }).FirstOrDefaultAsync();

            // If the campaign is not found, throw a `NotFoundException`.
            if (campaign is null)
                throw new NotFoundException("Campaign not found!");

            // Return the campaign record with associated user and email details.
            return campaign;
        }

        /// <summary>
        /// Inserts a new <see cref="Campaign"/> record into the database, associating it with the specified user.
        /// Validates that the campaign data is not null and checks for the existence of the specified user.
        /// </summary>
        /// <param name="campaign">The campaign object containing the data to be inserted.</param>
        /// <param name="userId">The ID of the user creating the campaign, which will be recorded as the campaign's creator.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">Thrown if the provided campaign data is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the specified user does not exist or lacks permissions.</exception>
        public async Task InsertAsync(Campaign campaign, int userId)
        {
            if (campaign is null)
                throw new NotFoundException("Campaign is empty or null!");

            // Enforce specific user logic (e.g., check user existence or permissions)
            bool userExists = await _context.UserInfo.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new UnauthorizedAccessException("User does not exist or does not have permissions.");

            // Set CreatedByUserId to the current user ID
            campaign.CreatedByUserId = userId;

            // Add and save the new Campaign record to the database
            _context.Add(campaign);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing `Campaign` record in the database.
        /// </summary>
        /// <param name="campaign">The campaign object containing updated data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">Thrown if the specified campaign does not exist in the database.</exception>
        /// <exception cref="DbConcurrencyException">Thrown if a concurrency issue occurs while updating the database.</exception>
        public async Task UpdateAsync(Campaign campaign)
        {
            //// Ensure the Campaign record exists before updating
            //bool hasAny = await _context.Campaign.AnyAsync(cm => cm.Id == campaign.Id);
            //if (!hasAny)
            //    throw new NotFoundException("Id(Campaign) not found!");

            // Check if the campaign exists in the database
            Campaign? existingCampaign = await _context.Campaign.FindAsync(campaign.Id);
            if (existingCampaign is null)
                throw new NotFoundException("Campaign not found.");

            try
            {
                //// Track changes and save the modified campaign entity
                //_context.Entry(campaign).State = EntityState.Modified;
                // _context.Update(campaign);

                // Update only the fields that can be changed/modified
                existingCampaign.CampaignName = campaign.CampaignName;
                existingCampaign.Description = campaign.Description;
                existingCampaign.StartDate = campaign.StartDate;
                existingCampaign.EndDate = campaign.EndDate;
                existingCampaign.IsActive = campaign.IsActive;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DbConcurrencyException(ex.Message);
            }
        }

        /// <summary>
        /// Removes a `Campaign` record by its ID, ensuring no associated `SendsEmails` entries are present.
        /// </summary>
        /// <param name="id">The unique ID of the campaign to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">Thrown if the specified campaign does not exist.</exception>
        /// <exception cref="IntegrityException">Thrown if deletion fails due to existing related `SendsEmails` entries.</exception>
        public async Task RemoveAsync(int id)
        {
            // Find the Campaign record to delete by its ID
            Campaign? campaign = await _context.Campaign.FindAsync(id)
                    ?? throw new NotFoundException("Campaign not found!");

            //// Manually check for related SendsEmails to simulate foreign key constraint
            //// InMemory provider does not fully support cascade delete behaviors in tests, this is only for unit tests
            //bool hasRelatedEmails = await _context.SendsEmails.AnyAsync(email => email.CampaignId == id);
            //if (hasRelatedEmails)
            //{
            //    throw new IntegrityException("Can't delete Campaign because it has Sends Emails!");
            //}

            // Attempt deletion; handle constraint violations if related SendsEmails exist
            try
            {
                _context.Campaign.Remove(campaign);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new IntegrityException("Can't delete Campaign because has Sends Emails!");
            }
        }
    }
}

