using CustomerDataDomainModels.Models;

namespace CustomerDataService.Services.Interfaces
{
    /// <summary>
    /// Interface for managing customer data operations, including retrieval, insertion, update, and deletion.
    /// </summary>
    public interface ICustomerDataService
    {
        /// <summary>
        /// Retrieves a list of all customers, including their associated email data.
        /// </summary>
        /// <returns>A list of <see cref="CustomerData"/> objects representing all customers.</returns>
        public Task<List<CustomerData>> FindAllAsync();

        /// <summary>
        /// Retrieves a specific customer by their unique identifier, including their associated emails.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <returns>The <see cref="CustomerData"/> object if found; otherwise, throws an exception.</returns>
        public Task<CustomerData> FindByIdAsync(int id);

        /// <summary>
        /// Inserts a new customer record into the database.
        /// </summary>
        /// <param name="customerData">The <see cref="CustomerData"/> object containing customer information to insert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InsertAsync(CustomerData customerData);

        /// <summary>
        /// Updates an existing customer record in the database.
        /// </summary>
        /// <param name="customerData">The <see cref="CustomerData"/> object with updated information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateAsync(CustomerData customerData);

        /// <summary>
        /// Deletes a customer record by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RemoveAsync(int id);
    }
}

