using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Data;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerDataService.Services
{
    /// <summary>
    /// Provides methods for managing `SendsEmails` records, including retrieving, adding, updating, 
    /// and deleting email entries with pagination, date filtering, and relational data handling.
    /// </summary>
    public class SendsEmailsService : ISendsEmailsService
    {
        private readonly CustomerDataContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="SendsEmailsService"/> with the specified database context.
        /// </summary>
        /// <param name="customerDataContext">Database context for managing email and related data.</param>
        public SendsEmailsService(CustomerDataContext customerDataContext)
        {
            _context = customerDataContext;
        }

        /// <summary>
        /// Retrieves a paginated list of all `SendsEmails` records, including related CustomerData and Campaign, and user entities.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default is 1).</param>
        /// <param name="pageSize">Number of records per page (default is 10).</param>
        /// <returns>List of email DTOs for the specified page.</returns>
        public async Task<List<SendsEmailsDto>> FindAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            List<SendsEmails>? emails = await _context.SendsEmails
                .Include(se => se.CustomerData)
                .Include(se => se.Campaign)
                    .ThenInclude(c => c!.CreatedByUser) // Include user who created the campaign
                .OrderByDescending(se => se.Id)
                .ThenByDescending(se => se.SentDate) // Enforces descending date order, consistently sorted from newest to oldest based on SentDate
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map SendsEmails entities to SendsEmailsDto
            return emails.Select(email => new SendsEmailsDto
            {
                Id = email.Id,
                RecipientEmail = email.RecipientEmail,
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Status = email.Status,
                CampaignId = email.CampaignId,
                CampaignName = email.Campaign?.CampaignName ?? "N/A", // Handle null case (Campaign)
                CustomerDataId = email.CustomerDataId,
                CustomerDataName = email.CustomerData?.FullName ?? "N/A", // Handle null case (CustomerData)

                // Add UserInfo properties from Campaign
                // Populate UserInfo properties if present
                CreatedByUsername = email.Campaign?.CreatedByUser?.Username,
                CreatedByFirstName = email.Campaign?.CreatedByUser?.FirstName,
                CreatedByLastName = email.Campaign?.CreatedByUser?.LastName,
                CreatedByEmail = email.Campaign?.CreatedByUser?.Email
            }).ToList();
        }

        /// <summary>
        /// Finds an email entry by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the email to retrieve.</param>
        /// <returns>The `SendsEmails` record if found; otherwise, throws a `NotFoundException`.</returns>
        public async Task<SendsEmailsDto> FindByIdAsync(int id)
        {
            //return await _context.SendsEmails
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(se => se.Id == id)
            //    ?? throw new NotFoundException("SendsEmails not found!");

            SendsEmails? email = await _context.SendsEmails
                .Include(se => se.CustomerData)
                .Include(se => se.Campaign)
                    .ThenInclude(c => c!.CreatedByUser)
                .FirstOrDefaultAsync(se => se.Id == id);

            if (email is null)
                throw new NotFoundException("SendsEmails not found!");

            return new SendsEmailsDto
            {
                Id = email.Id,
                RecipientEmail = email.RecipientEmail,
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Status = email.Status,
                CampaignId = email.CampaignId,
                CampaignName = email.Campaign?.CampaignName ?? "N/A",
                CustomerDataId = email.CustomerDataId,
                CustomerDataName = email.CustomerData?.FullName ?? "N/A",

                // UserInfo properties from Campaign
                CreatedByUsername = email.Campaign?.CreatedByUser?.Username,
                CreatedByFirstName = email.Campaign?.CreatedByUser?.FirstName,
                CreatedByLastName = email.Campaign?.CreatedByUser?.LastName,
                CreatedByEmail = email.Campaign?.CreatedByUser?.Email
            };
        }

        /// <summary>
        /// Finds emails within a specified date range, with optional pagination.
        /// </summary>
        /// <param name="minDate">The earliest date for filtering.</param>
        /// <param name="maxDate">The latest date for filtering.</param>
        /// <param name="pageNumber">Page number for pagination (default is 1).</param>
        /// <param name="pageSize">Number of records per page (default is 10).</param>
        /// <returns>List of email DTOs matching the specified date range and page.</returns>
        public async Task<List<SendsEmailsDto>> FindByDateAsync(DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<SendsEmails>? query = from obj in _context.SendsEmails select obj;

            // Apply date filters
            query = ApplyDateFilter(query, minDate, maxDate);

            // Apply pagination
            List<SendsEmails> pagedResult = await query
                .Include(se => se.CustomerData)
                .Include(se => se.Campaign)
                    .ThenInclude(c => c!.CreatedByUser)
                .OrderByDescending(se => se.Id)
                .ThenByDescending(se => se.SentDate) // Enforces descending date order, consistently sorted from newest to oldest based on SentDate
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map SendsEmails Entities to SendsEmailsDto
            // return the result
            return pagedResult.Select(email => new SendsEmailsDto
            {
                Id = email.Id,
                RecipientEmail = email.RecipientEmail,
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Status = email.Status,
                CampaignId = email.CampaignId,
                CampaignName = email.Campaign?.CampaignName ?? "N/A",
                CustomerDataId = email.CustomerDataId,
                CustomerDataName = email.CustomerData?.FullName ?? "N/A",

                // UserInfo properties from Campaign
                CreatedByUsername = email.Campaign?.CreatedByUser?.Username,
                CreatedByFirstName = email.Campaign?.CreatedByUser?.FirstName,
                CreatedByLastName = email.Campaign?.CreatedByUser?.LastName,
                CreatedByEmail = email.Campaign?.CreatedByUser?.Email
            }).ToList();
        }

        /// <summary>
        /// Finds emails for a specific customer and within a date range, with pagination.
        /// </summary>
        /// <param name="id">The of the custmoer for filtering.</param>
        /// <param name="minDate">The earliest date for filtering.</param>
        /// <param name="maxDate">The latest date for filtering.</param>
        /// <param name="pageNumber">Page number for pagination (default is 1).</param>
        /// <param name="pageSize">Number of records per page (default is 10).</param>
        /// <returns>List of email DTOs matching the specified date range and page.</returns>
        public async Task<List<SendsEmailsDto>> FindByDateCustomerDataIdAsync(int id, DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<SendsEmails> query = _context.SendsEmails.Where(se => se.CustomerDataId == id);

            query = ApplyDateFilter(query, minDate, maxDate);

            // Apply pagination
            List<SendsEmails> pagedResult = await query
                .Include(se => se.CustomerData)
                .Include(se => se.Campaign)
                    .ThenInclude(c => c!.CreatedByUser)
                .OrderByDescending(se => se.SentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map SendsEmails Entities to SendsEmailsDto
            return pagedResult.Select(email => new SendsEmailsDto
            {
                Id = email.Id,
                RecipientEmail = email.RecipientEmail,
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Status = email.Status,
                CampaignId = email.CampaignId,
                CampaignName = email.Campaign?.CampaignName ?? "N/A",
                CustomerDataId = email.CustomerDataId,
                CustomerDataName = email.CustomerData?.FullName ?? "N/A",

                // UserInfo properties from Campaign
                CreatedByUsername = email.Campaign?.CreatedByUser?.Username,
                CreatedByFirstName = email.Campaign?.CreatedByUser?.FirstName,
                CreatedByLastName = email.Campaign?.CreatedByUser?.LastName,
                CreatedByEmail = email.Campaign?.CreatedByUser?.Email
            }).ToList();
        }

        /// <summary>
        /// Gets the total count of `SendsEmails` records.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _context.SendsEmails.CountAsync();
            }
            catch (Exception ex) // Catch the generic Exception
            {
                // Wrap the generic exception in a DatabaseException for clarity
                throw new DatabaseException("An error occurred while counting SendsEmails records.", ex);
            }
        }

        /// <summary>
        /// Counts emails within a specific date range.
        /// </summary>
        public async Task<int> CountEmailsByDateRange(DateTime? minDate, DateTime? maxDate)
        {
            // IQueryable<SendsEmails>? result = from obj in _context.SendsEmails select obj;
            IQueryable<SendsEmails>? query = _context.SendsEmails;
            query = ApplyDateFilter(query, minDate, maxDate);


            return await query.CountAsync();  // Get the count of filtered emails
        }

        /// <summary>
        /// Counts emails for a specific customer within a date range.
        /// </summary>
        public async Task<int> CountEmailsByCustomerAndDateRange(int customerId, DateTime? minDate, DateTime? maxDate)
        {
            IQueryable<SendsEmails> query = _context.SendsEmails.Where(se => se.CustomerDataId == customerId);

            query = ApplyDateFilter(query, minDate, maxDate);

            return await query.CountAsync();  // Get the count of filtered emails
        }

        /// <summary>
        /// Inserts a new `SendsEmails` record after validating references to Campaign and CustomerData.
        /// </summary>
        public async Task InsertAsync(SendsEmails sendsEmails)
        {
            if (sendsEmails is null)
                throw new NotFoundException("SendsEmails is empty or null!");

            // Validate CampaignId
            bool campaignExists = await _context.Campaign.AnyAsync(cm => cm.Id == sendsEmails.CampaignId);
            if (!campaignExists)
                throw new NotFoundException($"Campaign with ID {sendsEmails.CampaignId} not found!");

            // Validate CustomerDataId (if provided)
            if (sendsEmails.CustomerDataId.HasValue)
            {
                bool customerExists = await _context.CustomerData.AnyAsync(cd => cd.Id == sendsEmails.CustomerDataId.Value);
                if (!customerExists)
                    throw new NotFoundException($"CustomerData with ID {sendsEmails.CustomerDataId.Value} not found!");
            }

            _context.Add(sendsEmails);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing `SendsEmails` record after validating related entities.
        /// </summary>
        public async Task UpdateAsync(SendsEmails updatedSendsEmails)
        {
            // Check if the SendsEmails entry exists
            SendsEmails? existingSendsEmails = await _context.SendsEmails.FirstOrDefaultAsync(se => se.Id == updatedSendsEmails.Id);
            if (existingSendsEmails is null)
                throw new NotFoundException("Id(SendsEmails) not found!");

            // Validate CampaignId
            bool campaignExists = await _context.Campaign.AnyAsync(cm => cm.Id == updatedSendsEmails.CampaignId);
            if (!campaignExists)
                throw new NotFoundException("Campaign not found!");

            // Validate CustomerDataId (if provided)
            if (updatedSendsEmails.CustomerDataId.HasValue)
            {
                bool customerExists = await _context.CustomerData.AnyAsync(cd => cd.Id == updatedSendsEmails.CustomerDataId.Value);
                if (!customerExists)
                    throw new NotFoundException("CustomerData not found!");
            }

            // Update properties of the existing entity
            existingSendsEmails.RecipientEmail = updatedSendsEmails.RecipientEmail;
            existingSendsEmails.Subject = updatedSendsEmails.Subject;
            existingSendsEmails.Content = updatedSendsEmails.Content;
            existingSendsEmails.SentDate = updatedSendsEmails.SentDate;
            existingSendsEmails.Status = updatedSendsEmails.Status;
            existingSendsEmails.CampaignId = updatedSendsEmails.CampaignId;
            existingSendsEmails.CustomerDataId = updatedSendsEmails.CustomerDataId;

            // Update the SendsEmails entity
            try
            {
                // _context.Entry(sendsEmails).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DbConcurrencyException(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a `SendsEmails` record by its ID, handling constraints or related data exceptions.
        /// </summary>
        public async Task RemoveAsync(int id)
        {
            SendsEmails? sendsEmails = await _context.SendsEmails.FindAsync(id)
                ?? throw new NotFoundException("SendsEmails not found!");

            try
            {
                _context.SendsEmails.Remove(sendsEmails);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new IntegrityException("Cannot delete SendsEmails because of related data or constraints!");
            }
        }

        /// <summary>
        /// Applies date range filtering to an IQueryable of SendsEmails.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="minDate">The minimum date (inclusive).</param>
        /// <param name="maxDate">The maximum date (inclusive).</param>
        /// <returns>The filtered query.</returns>
        private IQueryable<SendsEmails> ApplyDateFilter(IQueryable<SendsEmails> query, DateTime? minDate, DateTime? maxDate)
        {
            if (minDate.HasValue)
            {
                query = query.Where(se => se.SentDate >= minDate.Value);
            }

            if (maxDate.HasValue)
            {
                query = query.Where(se => se.SentDate <= maxDate.Value);
            }

            return query;
        }
    }
}

