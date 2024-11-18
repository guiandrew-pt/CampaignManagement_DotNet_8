using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;

namespace CustomerDataService.Services.Interfaces
{
    /// <summary>
    /// Interface for managing email sending operations, including CRUD operations and 
    /// filtering based on date, customer, and pagination.
    /// </summary>
    public interface ISendsEmailsService
    {
        /// <summary>
        /// Retrieves a paginated list of all sent emails.
        /// </summary>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A list of <see cref="SendsEmailsDto"/> representing sent emails.</returns>
        public Task<List<SendsEmailsDto>> FindAllAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves a specific email record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the email.</param>
        /// <returns>The <see cref="SendsEmailsDto"/> object if found; otherwise, throws an exception.</returns>
        public Task<SendsEmailsDto> FindByIdAsync(int id);

        /// <summary>
        /// Retrieves a list of sent emails within a specified date range.
        /// </summary>
        /// <param name="minDate">The start date of the range (inclusive).</param>
        /// <param name="maxDate">The end date of the range (inclusive).</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A list of <see cref="SendsEmailsDto"/> objects within the specified date range.</returns>
        public Task<List<SendsEmailsDto>> FindByDateAsync(DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves a list of sent emails associated with a specific customer within a date range.
        /// </summary>
        /// <param name="id">The customer ID to filter by.</param>
        /// <param name="minDate">The start date of the range (inclusive).</param>
        /// <param name="maxDate">The end date of the range (inclusive).</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A list of <see cref="SendsEmailsDto"/> objects matching the filters.</returns>
        public Task<List<SendsEmailsDto>> FindByDateCustomerDataIdAsync(int id, DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves the total count of sent emails in the database.
        /// </summary>
        /// <returns>The total number of sent emails as an integer.</returns>
        public Task<int> GetTotalCountAsync();

        /// <summary>
        /// Counts the total number of sent emails within a specific date range.
        /// </summary>
        /// <param name="minDate">The start date of the range (inclusive).</param>
        /// <param name="maxDate">The end date of the range (inclusive).</param>
        /// <returns>The number of emails within the specified date range.</returns>
        public Task<int> CountEmailsByDateRange(DateTime? minDate, DateTime? maxDate);

        /// <summary>
        /// Counts the total number of sent emails associated with a specific customer and within a date range.
        /// </summary>
        /// <param name="customerID">The customer ID to filter by.</param>
        /// <param name="minDate">The start date of the range (inclusive).</param>
        /// <param name="maxDate">The end date of the range (inclusive).</param>
        /// <returns>The number of emails associated with the customer within the date range.</returns>
        public Task<int> CountEmailsByCustomerAndDateRange(int customerID, DateTime? minDate, DateTime? maxDate);

        /// <summary>
        /// Inserts a new email record into the database.
        /// </summary>
        /// <param name="sendsEmails">The email entity to insert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InsertAsync(SendsEmails sendsEmails);

        /// <summary>
        /// Updates an existing email record in the database.
        /// </summary>
        /// <param name="sendsEmails">The email entity with updated information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateAsync(SendsEmails sendsEmails);

        /// <summary>
        /// Deletes an email record based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the email to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RemoveAsync(int id);
    }
}

