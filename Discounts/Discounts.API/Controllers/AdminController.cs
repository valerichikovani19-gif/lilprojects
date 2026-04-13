// Copyright (C) TBC Bank. All Rights Reserved.
using Asp.Versioning;
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Offer;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrator")]//marto adminma
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IAuthService _authService;
        private readonly IGlobalSettingService _settingsService;

        public AdminController(IOfferService offerService, IAuthService authService, IGlobalSettingService settingsService)
        {
            _offerService = offerService;
            _authService = authService;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Retrieves all offers that are waiting for approval
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of pending offers</returns>
        /// <response code="200">Returns the list of pending offers</response>
        [HttpGet("offers/pending")]
        [ProducesResponseType(typeof(List<OfferDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingOffers(CancellationToken cancellationToken)
        {
            var offers = await _offerService.GetPendingOffersAsync(cancellationToken).ConfigureAwait(false);
            return Ok(offers);
        }

        /// <summary>
        /// Approves a pending offer, making it visible to customers
        /// </summary>
        /// <param name="id">The unique identifier of the offer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message.</returns>
        /// <response code="200">The offer was successfully approved</response>
        /// <response code="404">The offer was not found</response>
        [HttpPut("offers/{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveOffer(int id, CancellationToken cancellationToken)
        {
            await _offerService.ApproveOfferAsync(id, cancellationToken).ConfigureAwait(false);
            return Ok(new { Message = "Offer approved successfully" });
        }

        /// <summary>
        /// Rejects a pending offer with a specific reason
        /// </summary>
        /// <param name="id">The unique identifier of the offer</param>
        /// <param name="rejectDto">The rejection reason</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message.</returns>
        /// <response code="200">The offer was successfully rejected</response>
        /// <response code="404">The offer was not found</response>
        [HttpPut("offers/{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectOffer(int id, [FromBody] RejectOfferDto rejectDto, CancellationToken cancellationToken)
        {
            await _offerService.RejectOfferAsync(id, rejectDto, cancellationToken).ConfigureAwait(false);
            return Ok(new { Message = "Offer rejected" });
        }
        /// <summary>
        /// Retrieves the current global system settings
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The global settings object</returns>
        /// <response code="200">Returns the settings</response>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(GlobalSettingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var settings = await _settingsService.GetSettingsAsync(cancellationToken).ConfigureAwait(false);
            return Ok(settings);
        }

        /// <summary>
        /// Updates global system settings
        /// </summary>
        /// <param name="settingsDto">The new settings values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message.</returns>
        /// <response code="200">Settings were updated successfully</response>
        [HttpPut("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSettings([FromBody] GlobalSettingDto settingsDto, CancellationToken cancellationToken)
        {
            await _settingsService.UpdateSettingsAsync(settingsDto, cancellationToken).ConfigureAwait(false);
            return Ok(new { Message = "Global settings updated successfully" });
        }
        /// <summary>
        /// Gets a list of all registered users in the system
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await _authService.GetAllUsersAsync(cancellationToken).ConfigureAwait(false);
            return Ok(users);
        }

        /// <summary>
        /// Blocks a user, preventing them from logging in
        /// </summary>
        /// <param name="email">The email address of the user to block</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        /// <response code="200">User was successfully blocked</response>
        /// <response code="404">User not found</response>
        [HttpPut("users/{email}/block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BlockUser(string email, CancellationToken cancellationToken)
        {
            await _authService.BlockUserAsync(email, cancellationToken).ConfigureAwait(false);
            return Ok(new { Message = $"User {email} has been blocked" });
        }

        /// <summary>
        /// Unblocks a previously blocked user
        /// </summary>
        /// <param name="email">The email address of the user to unblock</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        /// <response code="200">User was successfully unblocked</response>
        /// <response code="404">User not found</response>
        [HttpPut("users/{email}/unblock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnblockUser(string email, CancellationToken cancellationToken)
        {
            await _authService.UnblockUserAsync(email, cancellationToken).ConfigureAwait(false);
            return Ok(new { Message = $"User {email} has been unblocked" });
        }

        /// <summary>
        /// Forcibly deletes an offer (Admin override)
        /// </summary>
        /// <param name="id">The unique identifier of the offer to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content on success</returns>
        /// <response code="204">The offer was successfully deleted</response>
        /// <response code="404">The offer was not found</response>
        [HttpDelete("offers/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOfferByAdmin(int id, CancellationToken cancellationToken)
        {
            await _offerService.DeleteOfferByAdminAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

    }
}
