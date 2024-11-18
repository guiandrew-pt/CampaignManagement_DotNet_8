using CustomerDataAPI.ViewModels;
using CustomerDataDomainModels.Models;
using CustomerDataDTOs.DTOs;
using CustomerDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerDataAPI.Controllers
{
    /// <summary>
    /// Controller for managing email records (SendsEmails) in the application.
    /// Provides endpoints for retrieving, creating, updating, and deleting email records, 
    /// with pagination and filtering by date and customer options. Authorization and CORS are enabled.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class SendsEmailsController : ControllerBase
    {
        private readonly ISendsEmailsService _sendsEmailsService;
        private readonly ICustomerDataService _customerDataService;
        // private readonly ILogger<SendsEmailsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendsEmailsController"/> class.
        /// </summary>
        /// <param name="sendsEmailsService">Service for email operations.</param>
        /// <param name="customerDataService">Service for customer data operations.</param>
        public SendsEmailsController(ISendsEmailsService sendsEmailsService, ICustomerDataService customerDataService /*, ILogger<SendsEmailsController> logger*/)
        {
            _sendsEmailsService = sendsEmailsService;
            _customerDataService = customerDataService;
            // _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of all email records.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of emails with metadata, or a bad request if pagination is invalid.</returns>
        // GET: api/SendsEmails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SendsEmails>>> GetAllEmails(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            List<SendsEmailsDto> emails = await _sendsEmailsService.FindAllAsync(pageNumber, pageSize);

            // Optionally, return pagination metadata
            // Get total count of emails
            int totalEmails = await _sendsEmailsService.GetTotalCountAsync();

            PaginatedResponse<SendsEmailsDto> paginatedResponse = new PaginatedResponse<SendsEmailsDto>(emails, pageNumber, pageSize, totalEmails);

            return Ok(paginatedResponse);
        }

        /// <summary>
        /// Retrieves a single email record by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the email.</param>
        /// <returns>Returns the requested email, or 404 if not found.</returns>
        // GET: api/<SendsEmailsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SendsEmailsDto>> GetSendsEmailsId(int id)
        {
            SendsEmailsDto? email = await _sendsEmailsService.FindByIdAsync(id);
            if (email is null)
            {
                return NotFound($"SendsEmails with ID {id} not found.");
            }

            return Ok(email);
        }

        /// <summary>
        /// Retrieves emails filtered by a date range, with optional pagination.
        /// </summary>
        /// <param name="minDate">The minimum date for filtering.</param>
        /// <param name="maxDate">The maximum date for filtering.</param>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of filtered emails, or an empty response if none match.</returns>
        // GET: api/<SendsEmailsController>/filterByData/5
        [HttpGet("filterByData")]
        public async Task<ActionResult<IEnumerable<SendsEmails>>> GetEmailsByDate(DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            if (minDate.HasValue && maxDate.HasValue && minDate > maxDate)
            {
                return BadRequest("minDate cannot be greater than maxDate.");
            }

            List<SendsEmailsDto>? emails = await _sendsEmailsService.FindByDateAsync(minDate, maxDate, pageNumber, pageSize);
            
            if (!emails.Any())
            {
                return Ok(new PaginatedResponse<SendsEmailsDto>(new List<SendsEmailsDto>(), pageNumber, pageSize, 0));
            }

            // Get the total count of filtered emails (if needed after filtering)
            int totalEmails = await _sendsEmailsService.CountEmailsByDateRange(minDate, maxDate);

            PaginatedResponse<SendsEmailsDto> paginatedResponse = new PaginatedResponse<SendsEmailsDto>(emails, pageNumber, pageSize, totalEmails);

            //return Ok(emails); // Return empty array if no emails found
            return Ok(paginatedResponse);
        }

        /// <summary>
        /// Retrieves emails filtered by customer and date range, with optional pagination.
        /// </summary>
        /// <param name="customerId">The ID of the customer to filter by.</param>
        /// <param name="minDate">The minimum date for filtering.</param>
        /// <param name="maxDate">The maximum date for filtering.</param>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of filtered emails, or a 404 if the customer is not found.</returns>
        // GET: api/<SendsEmailsController>/filterByCustomerData/5
        [HttpGet("filterByCustomerData/{customerId}")]
        public async Task<ActionResult<IEnumerable<SendsEmails>>> GetEmailsByCustomerAndDate(int customerId, DateTime? minDate, DateTime? maxDate, int pageNumber = 1, int pageSize = 10)
        {
            CustomerData? customer = await _customerDataService.FindByIdAsync(customerId);
            if (customer is null)
            {
                return NotFound($"Customer with ID {customerId} not found.");
            }

            if (minDate.HasValue && maxDate.HasValue && minDate > maxDate)
            {
                return BadRequest("minDate cannot be greater than maxDate.");
            }

            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            List<SendsEmailsDto>? emails = await _sendsEmailsService.FindByDateCustomerDataIdAsync(customerId, minDate, maxDate, pageNumber, pageSize);

            if (!emails.Any())
            {
                return Ok(new PaginatedResponse<SendsEmailsDto>(new List<SendsEmailsDto>(), pageNumber, pageSize, 0));
            }

            // Get the total count of filtered emails
            int totalEmails = await _sendsEmailsService.CountEmailsByCustomerAndDateRange(customerId, minDate, maxDate);

            PaginatedResponse<SendsEmailsDto> paginatedResponse = new PaginatedResponse<SendsEmailsDto>(emails, pageNumber, pageSize, totalEmails);

            return Ok(paginatedResponse);
        }

        /// <summary>
        /// Creates a new email record.
        /// </summary>
        /// <param name="sendsEmails">The email record to create.</param>
        /// <returns>Returns the created email with a 201 status, or 400 if validation fails.</returns>
        // POST: api/<SendsEmailsController>
        [HttpPost]
        public async Task<IActionResult> PostSendsEmails([FromBody] SendsEmails sendsEmails)
        {
            if (!ModelState.IsValid)
            {
                //// Log validation errors
                //foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    _logger.LogError("Validation error: {ErrorMessage}", error.ErrorMessage);
                //}

                return BadRequest(ModelState); // Validate the model
            }

            //if (!Enum.IsDefined(typeof(Status), sendsEmails.Status))
            //{
            //    return BadRequest("Invalid status value provided.");
            //}

            //// Log the incoming payload
            //_logger.LogInformation("Received SendsEmails payload: {@SendsEmails}", sendsEmails);

            await _sendsEmailsService.InsertAsync(sendsEmails);
            return CreatedAtAction(nameof(GetSendsEmailsId), new { id = sendsEmails.Id }, sendsEmails); // Return 201 Created
        }

        /// <summary>
        /// Updates an existing email record.
        /// </summary>
        /// <param name="id">The unique identifier of the email to update.</param>
        /// <param name="sendsEmails">The updated email record.</param>
        /// <returns>Returns the updated email or 404 if the email is not found.</returns>
        // PUT api/<SendsEmailsController>/5
        [Authorize(Roles = "Admin, Manager")] // Only Admins and Manager can delete
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSendsEmails(int id, [FromBody] SendsEmails sendsEmails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Validate the model
            }

            // Ensure the ID in the route matches the one in the body
            if (id != sendsEmails.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the body.");
            }

            // Check if the email exists before updating
            SendsEmailsDto? existingEmail = await _sendsEmailsService.FindByIdAsync(id);
            if (existingEmail is null)
            {
                return NotFound($"SendsEmails with ID {id} not found."); // Return 404 if not found
            }

            await _sendsEmailsService.UpdateAsync(sendsEmails);
            return Ok(await _sendsEmailsService.FindByIdAsync(sendsEmails.Id)); // Return the updated email
        }

        /// <summary>
        /// Deletes an email record by its ID. Only accessible to Admins.
        /// </summary>
        /// <param name="id">The unique identifier of the email to delete.</param>
        /// <returns>Returns 204 if successful, or 404 if the email is not found.</returns>
        // DELETE api/<SendsEmailsController>/5
        [Authorize(Roles = "Admin, Manager")] // Only Admins and Manager can delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSendsEmails(int id)
        {
            // Check if the email exists before delete
            SendsEmailsDto? existingEmail = await _sendsEmailsService.FindByIdAsync(id);
            if (existingEmail is null)
            {
                return NotFound($"SendsEmails with ID {id} not found."); // Return 404 if not found
            }

            // Proceed with deletion
            await _sendsEmailsService.RemoveAsync(id);
            return NoContent(); // Return 204 No Content
        }
    }
}
