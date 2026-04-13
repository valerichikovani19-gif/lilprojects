// Copyright (C) TBC Bank. All Rights Reserved.
using Asp.Versioning;
using Discounts.Application.DTOs.Offer;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Discounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        /// <summary>
        /// Gets all offers created by the currently logged-in merchant
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of the merchant's offers</returns>
        /// <response code="200">Returns the list of offers</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-offers")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(List<OfferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOffers(CancellationToken cancellationToken)
        {
            var merchantId = GetCurrentUserId();
            var offers = await _offerService.GetMerchantOffersAsync(merchantId, cancellationToken).ConfigureAwait(false);
            return Ok(offers);
        }

        /// <summary>
        /// Gets a specific offer by ID
        /// </summary>
        /// <param name="id">The unique identifier of the offer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The offer details</returns>
        /// <response code="200">Returns the offer</response>
        /// <response code="404">Offer not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OfferDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {

            var offer = await _offerService.GetOfferByIdAsync(id, cancellationToken).ConfigureAwait(false);
            return Ok(offer);
        }

        /// <summary>
        /// Creates a new offer (Status will be Pending)
        /// </summary>
        /// <param name="offerDto">Offer creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created offer with its ID</returns>
        /// <response code="201">Offer created successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(OfferDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateOfferDto offerDto, CancellationToken cancellationToken)
        {

            var merchantId = GetCurrentUserId();
            var id = await _offerService.CreateOfferAsync(offerDto, merchantId, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = id }, new { id = id });

        }

        /// <summary>
        /// Updates an existing offer
        /// </summary>
        /// <param name="id">The ID of the offer to update</param>
        /// <param name="offerDto">The updated offer details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        /// <response code="204">Update successful</response>
        /// <response code="404">Offer not found</response>
        /// <response code="403">Forbidden (Edit window closed or not owner)</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferDto offerDto, CancellationToken cancellationToken)
        {
            var merchantId = GetCurrentUserId();
            await _offerService.UpdateOfferAsync(id, offerDto, merchantId, cancellationToken).ConfigureAwait(false);
            return NoContent();

        }
        /// <summary>
        /// Deletes an offer (Soft Delete)
        /// </summary>
        /// <param name="id">The ID of the offer to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content on success</returns>
        /// <response code="204">The offer was successfully deleted</response>
        /// <response code="404">The offer was not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var merchantId = GetCurrentUserId();
            await _offerService.DeleteOfferAsync(id, merchantId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        /// <summary>
        /// Gets the sales history for the logged-in merchant
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of sales transactions</returns>
        /// <response code="200">Returns the sales history</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("sales-history")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(List<MerchantSalesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesHistory(CancellationToken cancellationToken)
        {
            var merchantId = GetCurrentUserId();
            var history = await _offerService.GetMerchantSalesHistoryAsync(merchantId, cancellationToken).ConfigureAwait(false);
            return Ok(history);
        }
        /// <summary>
        /// Gets all active (approved) offers. Public endpoint
        /// </summary>
        /// <param name="search">Optional keyword to filter by title or description</param>
        /// <param name="categoryId">Optional category ID filter</param>
        /// <param name="minPrice">Optional minimum price filter</param>
        /// <param name="maxPrice">Optional maximum price filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of active offers</returns>
        /// <response code="200">Returns the filtered list of offers</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<OfferDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllActive(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
        {
            var offers = await _offerService.GetActiveOffersAsync(
                search, categoryId, minPrice, maxPrice, pageIndex, pageSize, cancellationToken).ConfigureAwait(false);

            return Ok(offers);
        }
        /// <summary>
        /// Gets statistics for the merchant dashboard (Revenue, Sales, Active Offers)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        ///<returns></returns>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var merchantId = GetCurrentUserId();
            var stats = await _offerService.GetMerchantDashboardAsync(merchantId, cancellationToken).ConfigureAwait(false);
            return Ok(stats);
        }

        // helperi useridis jwtad dasaextracteblad
        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                //middleware daichers
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }
}
