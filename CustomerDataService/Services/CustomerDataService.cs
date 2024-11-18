using CustomerDataDomainModels.Models;
using CustomerDataService.Data;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerDataService.Services
{
    /// <summary>
    /// Service class for managing `CustomerData` records, providing operations to 
    /// retrieve, add, update, and delete customers, along with related email data.
    /// </summary>
    public class CustomerDataService : ICustomerDataService
    {
        private readonly CustomerDataContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="CustomerDataService"/> with the specified database context.
        /// </summary>
        /// <param name="customerDataContext">Database context for customer and related data.</param>
        public CustomerDataService(CustomerDataContext customerDataContext)
        {
            _context = customerDataContext;
        }

        /// <summary>
        /// Retrieves all `CustomerData` records, including their associated `SendsEmails` entries.
        /// </summary>
        /// <returns>A list of all customer records, each with a collection of emails sent.</returns>
        public async Task<List<CustomerData>> FindAllAsync()
        {
            // Include EmailsSent to retrieve associated emails with each CustomerData record.
            return await _context.CustomerData.Include(cd => cd.EmailsSent).ToListAsync();
        }

        /// <summary>
        /// Finds a `CustomerData` record by its unique identifier (ID), including associated emails.
        /// </summary>
        /// <param name="id">The ID of the customer to retrieve.</param>
        /// <returns>The customer record if found, otherwise throws a `NotFoundException`.</returns>
        public async Task<CustomerData> FindByIdAsync(int id)
        {
            return await _context.CustomerData
                .AsNoTracking() // Avoids tracking to optimize read-only operations
                .Include(cd => cd.EmailsSent) // Eager loading of related `SendsEmails`
                .FirstOrDefaultAsync(cd => cd.Id == id) // Search for customer by ID
                ?? throw new NotFoundException("CustomerData not found!");
        }

        /// <summary>
        /// Inserts a new `CustomerData` record into the database.
        /// </summary>
        /// <param name="customerData">The customer data to insert.</param>
        /// <exception cref="NotFoundException">Thrown if the provided customer data is null.</exception>
        public async Task InsertAsync(CustomerData customerData)
        {
            if (customerData is null)
                throw new ArgumentNullException("CustomerData is empty or null!");

            if (await EmailExistsAsync(customerData.Email!))
                throw new InvalidOperationException("A customer with this email already exists.");

            // Add and save the new CustomerData record to the database
            _context.Add(customerData);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing `CustomerData` record in the database.
        /// </summary>
        /// <param name="customerData">The customer data with updated values.</param>
        /// <exception cref="NotFoundException">Thrown if the customer record to update does not exist.</exception>
        /// <exception cref="DbConcurrencyException">Thrown if there is a concurrency issue during update.</exception>
        public async Task UpdateAsync(CustomerData customerData)
        {
            if (customerData is null)
                throw new ArgumentNullException(nameof(customerData), "CustomerData cannot be null!");

            if (string.IsNullOrWhiteSpace(customerData.Email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(customerData.Email));

            // Verify if the CustomerData record exists
            bool exists = await _context.CustomerData.AnyAsync(cd => cd.Id == customerData.Id);
            if (!exists)
                throw new NotFoundException($"CustomerData with ID {customerData.Id} not found!");

            // Check if the email is already used by another customer
            bool emailConflict = await _context.CustomerData
                .AnyAsync(c => c.Email == customerData.Email && c.Id != customerData.Id);
            if (emailConflict)
                throw new InvalidOperationException("This email is already associated with another customer.");

            // Set the customerData entity's state to Modified to track changes for update
            // Update the customerData entity
            try
            {
                _context.Entry(customerData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DbConcurrencyException(ex.Message);
            }
        }

        /// <summary>
        /// Removes a `CustomerData` record by its ID, ensuring no associated `SendsEmails` entries are present.
        /// </summary>
        /// <param name="id">The ID of the customer to remove.</param>
        /// <exception cref="NotFoundException">Thrown if the specified customer does not exist.</exception>
        /// <exception cref="IntegrityException">Thrown if deletion fails due to existing related `SendsEmails` entries.</exception>
        public async Task RemoveAsync(int id)
        {
            // Locate the CustomerData record to delete
            CustomerData? customerData = await _context.CustomerData.FindAsync(id)
                    ?? throw new NotFoundException("CustomerData not found!");

            // Attempt deletion; handle constraint violations (e.g., if related SendsEmails exist)
            try
            {
                _context.CustomerData.Remove(customerData);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new IntegrityException("Can't delete Customer Data because has Sends Emails!");
            }
        }

        /// <summary>
        /// Checks whether a customer with the specified email address exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>
        private async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");

            return await _context.CustomerData.AnyAsync(c => c.Email == email);
        }
    }
}

