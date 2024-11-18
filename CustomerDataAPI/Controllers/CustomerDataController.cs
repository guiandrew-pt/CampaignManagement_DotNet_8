using CustomerDataDomainModels.Models;
using CustomerDataService.Services.Exceptions;
using CustomerDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerDataAPI.Controllers
{
    /// <summary>
    /// API Controller for managing customer data in the application.
    /// Provides endpoints for retrieving, creating, updating, and deleting customer data records.
    /// Enables CORS for communication with client applications and restricts delete operations to Admins.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class CustomerDataController : ControllerBase
    {
        // Simulate in-memory storage
        //private static List<CustomerData> customerDataStore = new List<CustomerData>();

        private readonly ICustomerDataService _customerDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDataController"/> class.
        /// </summary>
        /// <param name="customerDataService">Service for customer data operations.</param>
        public CustomerDataController(ICustomerDataService customerDataService)
        {
            _customerDataService = customerDataService;
        }

        /// <summary>
        /// Retrieves all customer data records.
        /// </summary>
        /// <returns>A list of all customer data records, or 404 if none are found.</returns>
        // GET: CustomerData (Index)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerData>>> GetAllCustomersData()
        {
            List<CustomerData> customers = await _customerDataService.FindAllAsync();

            if (customers is null)
            {
                return NotFound($"CustomerData not found.");
            }

            return Ok(customers);
        }

        /// <summary>
        /// Retrieves a single customer data record by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the customer record to retrieve.</param>
        /// <returns>The requested customer data, or 404 if not found.</returns>
        // GET: api/<CustomerDataController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerData>> GetCustomersData(int id)
        {
            CustomerData? customer = await _customerDataService.FindByIdAsync(id);
            if (customer is null)
            {
                return NotFound($"CustomerData with ID {id} not found.");
            }

            return Ok(customer);
        }

        /// <summary>
        /// Creates a new customer data record.
        /// </summary>
        /// <param name="customerData">The customer data to create.</param>
        /// <returns>The created customer data record with a 201 Created status, or 400 if validation fails.</returns>
        // POST: api/<CustomerDataController>
        [HttpPost]
        public async Task<IActionResult> PostCustomerData([FromBody] CustomerData customerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Validate the model
            }

            try
            {
                await _customerDataService.InsertAsync(customerData);
                return CreatedAtAction(nameof(GetCustomersData), new { id = customerData.Id }, customerData); // Return 201 Created
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                return Conflict("A customer with this email already exists."); // Return 409 Conflict with specific message
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing customer data record.
        /// </summary>
        /// <param name="id">The unique identifier of the customer data record to update.</param>
        /// <param name="customerData">The updated customer data.</param>
        /// <returns>The updated customer data record, or 404 if the customer does not exist.</returns>
        // PUT api/<CustomerDataController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerData(int id, [FromBody] CustomerData customerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid input data. Please review the errors and try again."); // Validate the model
            }

            // Check if the customer exists before updating
            CustomerData? existingCustomer = await _customerDataService.FindByIdAsync(id);
            if (existingCustomer is null)
            {
                return NotFound($"CustomerData with ID {id} not found."); // Return 404 if not found
            }

            try
            {
                await _customerDataService.UpdateAsync(customerData);
                return Ok(await _customerDataService.FindByIdAsync(customerData.Id)); // Return the updated customer
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message); // Return 404 with the specific error message
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // Return 409 Conflict with a specific error
            }
            catch (DbConcurrencyException ex)
            {
                return StatusCode(500, ex.Message); // Return 500 Internal Server Error with a specific message
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}"); // Fallback for unexpected errors
            }
        }

        /// <summary>
        /// Deletes a customer data record by its ID. Only accessible to Admins.
        /// </summary>
        /// <param name="id">The unique identifier of the customer data to delete.</param>
        /// <returns>204 No Content if successful, or 404 if the customer is not found.</returns>
        // DELETE api/<CustomerDataController>/5
        [Authorize(Roles = "Admin, Manager")] // Only Admins and Manager can delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerData(int id)
        {
            // Check if the customer exists before delete
            CustomerData? existingCustomer = await _customerDataService.FindByIdAsync(id);
            if (existingCustomer is null)
            {
                return NotFound($"CustomerData with ID {id} not found."); // Return 404 if not found
            }

            // Proceed with deletion
            await _customerDataService.RemoveAsync(id);
            return NoContent(); // Return 204 No Content
        }

        //[HttpPost]
        //public IActionResult PostCustomerData([FromBody] CustomerData customerData)
        //{
        //    if (customerData == null || !ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid customer data.");
        //    }

        //    // Add data to in-memory store
        //    customerDataStore.Add(customerData);
        //    return Ok("Customer data saved.");
        //}

        //[HttpGet]
        //public IActionResult GetAllCustomerData()
        //{
        //    return Ok(customerDataStore);
        //}
    }
}
