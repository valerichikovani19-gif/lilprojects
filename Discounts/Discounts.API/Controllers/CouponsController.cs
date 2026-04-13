// Copyright (C) TBC Bank. All Rights Reserved.
using Asp.Versioning;
using Discounts.Application.DTOs.Coupon;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Discounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize] //mxolod daloginebul userebs sheudzliat kidva
    [Produces("application/json")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }
        /// <summary>
        /// Reserves a coupon for a limited time (Booking)
        /// </summary>
        /// <remarks>
        /// Decreases available quantity by 1. The reservation expires automatically if not purchased within the time limit
        /// </remarks>
        /// <param name="offerId">The ID of the offer to reserve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The reserved coupon details</returns>
        /// <response code="200">Reservation successful</response>
        /// <response code="404">Offer not found</response>
        /// <response code="400">Offer is not active, expired, or out of stock</response>
        [HttpPost("reserve/{offerId}")]
        [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reserve(int offerId, CancellationToken cancellationToken)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //marags erti akldeba da statusi reserved xdeba
            var coupon = await _couponService.ReserveCouponAsync(offerId, customerId!, cancellationToken).ConfigureAwait(false);

            return Ok(coupon);
        }

        /// <summary>
        /// Purchases a coupon immediately (Buy Now)
        /// </summary>
        /// <param name="offerId">The ID of the offer to purchase</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The purchased coupon details</returns>
        /// <response code="200">Purchase successful</response>
        /// <response code="404">Offer not found</response>
        /// <response code="400">Offer is not active, expired, or out of stock </response>
        [HttpPost("purchase/{offerId}")]
        [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Purchase(int offerId, CancellationToken cancellationToken)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //anu ak axla trycatch rakiagarmak soldout -stockdepleted
            //expired - OfferNotActive  da bad id OferNotFound
            var coupon = await _couponService.PurchaseCouponAsync(offerId, customerId!, cancellationToken).ConfigureAwait(false);
            return Ok(coupon);

        }
        /// <summary>
        /// Gets the current user's purchase history
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of coupons purchased/reserved by the user</returns>
        /// <response code="200">Returns the list of coupons</response>
        [HttpGet("my-history")]
        [ProducesResponseType(typeof(List<CouponDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyHistory(CancellationToken cancellationToken)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var coupons = await _couponService.GetMyCouponsAsync(customerId!, cancellationToken).ConfigureAwait(false);
            return Ok(coupons);
        }
        /// <summary>
        /// Redeems a customer's coupon code at the physical location (Merchant only)
        /// </summary>
        /// <param name="code">The 8-character coupon code provided by the customer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message upon validation</returns>
        /// <response code="200">Coupon validated and redeemed successfully</response>
        /// <response code="400">Coupon is invalid, expired, or not in a purchased state</response>
        /// <response code="404">Coupon not found</response>
        [HttpPost("redeem/{code}")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Redeem(string code, CancellationToken cancellationToken)
        {
            var merchantId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _couponService.RedeemCouponAsync(code, merchantId!, cancellationToken).ConfigureAwait(false);

            return Ok(new { Message = "Coupon validated and redeemed successfully!" });
        }
    }
}
