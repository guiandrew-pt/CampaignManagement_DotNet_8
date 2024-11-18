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
    /// API Controller for managing Campaign data in the application.
    /// Provides endpoints to retrieve, create, update, and delete campaign records.
    /// Enables CORS for client communication and restricts delete operations to Admins.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowBlazorApp")]
    [Authorize]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignController"/> class.
        /// </summary>
        /// <param name="campaignService">Service for campaign operations.</param>
        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        /// <summary>
        /// Retrieves all campaign records.
        /// </summary>
        /// <returns>A list of all campaign records, or 404 if none are found.</returns>
        // GET: Campaign (Index)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetAllCampaigns()
        {
            List<CampaignDto> campaigns = await _campaignService.FindAllAsync();

            if (campaigns is null)
            {
                return NotFound($"Campaign not found.");
            }

            return Ok(campaigns);
        }

        /// <summary>
        /// Retrieves a specific campaign record by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign to retrieve.</param>
        /// <returns>The requested campaign, or 404 if not found.</returns>
        // GET: api/<CampaignController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CampaignDto>> GetCampaign(int id)
        {
            CampaignDto? campaign = await _campaignService.FindByIdAsync(id);
            if (campaign is null)
            {
                return NotFound($"Campaign with ID {id} not found.");
            }

            return Ok(campaign);
        }

        /// <summary>
        /// Creates a new campaign record.
        /// </summary>
        /// <param name="campaign">The campaign data to create.</param>
        /// <returns>The created campaign record with a 201 Created status, or 400 if validation fails.</returns>
        // POST: api/<CampaignController>
        [HttpPost]
        public async Task<IActionResult> PostCampaign([FromBody] Campaign campaign)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Validate the model
            }

            // Retrieve the logged-in user's ID from claims
            string? userIdClaim = User.FindFirst("id")?.Value;

            // Check if the userIdClaim is valid and parse it to an integer
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid or missing user ID in token."); // Return 401 if invalid
            }

            // Pass the campaign and userId to the service
            await _campaignService.InsertAsync(campaign, userId);
            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign); // Return 201 Created
        }

        /// <summary>
        /// Updates an existing campaign record.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign to update.</param>
        /// <param name="campaign">The updated campaign data.</param>
        /// <returns>The updated campaign record, or 404 if the campaign does not exist.</returns>
        // PUT api/<CampaignController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] Campaign campaign)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Validate the model
            }

            // Check if the campaign exists before updating
            // Retrieve the current campaign to ensure it exists and prevent updating non-mutable fields
            CampaignDto? existingCampaign = await _campaignService.FindByIdAsync(id);
            if (existingCampaign is null)
            {
                return NotFound($"Campaign with ID {id} not found."); // Return 404 if not found
            }

            // Ensure that the ID from the URL is assigned to the entity // Testing only
            // campaign.Id = id;

            // Perform the update without changing CreatedByUserId
            await _campaignService.UpdateAsync(campaign);
            return Ok(await _campaignService.FindByIdAsync(campaign.Id)); // Return the updated campaign
        }

        /// <summary>
        /// Deletes a campaign record by ID. Only accessible to Admins.
        /// </summary>
        /// <param name="id">The unique identifier of the campaign to delete.</param>
        /// <returns>204 No Content if successful, or 404 if the campaign is not found.</returns>
        // DELETE api/<CampaignController>/5
        [Authorize(Roles = "Admin, Manager")] // Only Admins and Manager can delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            // Check if the campaign exists before delete
            CampaignDto? existingCampaign = await _campaignService.FindByIdAsync(id);
            if (existingCampaign is null)
            {
                return NotFound($"Campaign with ID {id} not found."); // Return 404 if not found
            }

            // Proceed with deletion
            await _campaignService.RemoveAsync(id);
            return NoContent(); // Return 204 No Content
        }
    }
}
