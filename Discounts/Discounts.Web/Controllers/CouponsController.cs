// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Discounts.Web.Controllers
{
    [Authorize]
    public class CouponsController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // POST : /Coupons/Purchase
        [HttpPost]
        public async Task<IActionResult> Purchase(int offerId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _couponService.PurchaseCouponAsync(offerId, userId!, cancellationToken).ConfigureAwait(false);

                TempData["SuccessMessage"] = "Coupon purchased successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Home", new { id = offerId });
            }
        }

        //POST: /Coupons/Reserve
        [HttpPost]
        public async Task<IActionResult> Reserve(int offerId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _couponService.ReserveCouponAsync(offerId, userId!, cancellationToken).ConfigureAwait(false);

                TempData["SuccessMessage"] = "Coupon reserved! You have 30 minutes to buy it";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Home", new { id = offerId });
            }
        }

        // GET: /Coupons/Index (my coupons page)
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myCoupons = await _couponService.GetMyCouponsAsync(userId!, cancellationToken).ConfigureAwait(false);

            return View(myCoupons);
        }
    }
}
