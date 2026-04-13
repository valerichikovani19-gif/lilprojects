// Copyright (C) TBC Bank. All Rights Reserved.
using System.Security.Claims;
using System.Text;
using Discounts.Application.DTOs.Offer;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly ICategoryService _categoryService;
        private readonly ICouponService _couponService;

        private readonly IGlobalSettingService _globalSettingService;

        public MerchantController(IOfferService offerService, ICategoryService categoryService, IGlobalSettingService globalSettingService, ICouponService couponService)
        {
            _offerService = offerService;
            _categoryService = categoryService;
            _globalSettingService = globalSettingService;
            _couponService = couponService;

        }

        //GET : /Merchant/Dashboard
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dashboardStats = await _offerService.GetMerchantDashboardAsync(userId!, cancellationToken).ConfigureAwait(false);
            return View(dashboardStats);
        }

        // GET: /Merchant/MyOffers
        public async Task<IActionResult> MyOffers(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var offers = await _offerService.GetMerchantOffersAsync(userId!, cancellationToken).ConfigureAwait(false);
            return View(offers);
        }
        // GET:/Merchant/SalesHistory
        public async Task<IActionResult> SalesHistory(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var history = await _offerService.GetMerchantSalesHistoryAsync(userId!, cancellationToken).ConfigureAwait(false);
            return View(history);
        }
        //GET: /Merchant/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            //dropdownistvis 
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        // POST:/ Merchant/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateOfferDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.CreateOfferAsync(model, userId!, cancellationToken).ConfigureAwait(false);

                TempData["SuccessMessage"] = "Offer created successfully! It is now Pending approval";
                return RedirectToAction("MyOffers");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }
        }
        // GET: /Merchant/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var offer = await _offerService.GetOfferByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (offer == null) return NotFound();

            if (offer.Status != "Pending")
            {
                var settings = await _globalSettingService.GetSettingsAsync(cancellationToken).ConfigureAwait(false);
                var windowHours = settings.MerchantEditWindowInHours;

                var createdTime = offer.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : offer.CreatedAt;
                var hoursSinceCreation = (DateTime.UtcNow - createdTime).TotalHours;

                if (hoursSinceCreation > windowHours)
                {
                    TempData["ErrorMessage"] = $"Time limit exceeded - You can only edit Active offers within {windowHours} hours.";
                    return RedirectToAction("MyOffers");
                }
            }

            // 3. Map to DTO
            var updateDto = new UpdateOfferDto
            {
                Title = offer.Title,
                Description = offer.Description,
                OriginalPrice = offer.OriginalPrice,
                DiscountPrice = offer.DiscountPrice,
                TotalQuantity = offer.AvailableQuantity,
                ValidFrom = offer.ValidFrom,
                ValidUntil = offer.ValidUntil,
                CategoryId = offer.CategoryId,
                ImageUrl = offer.ImageUrl
            };

            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
            ViewBag.Categories = new SelectList(categories, "Id", "Name", offer.CategoryId);
            ViewBag.OfferId = id;

            return View(updateDto);
        }
        //POST: /Merchant/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateOfferDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                //refetchva
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
                ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
                ViewBag.OfferId = id;

                return View(model);
            }
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.UpdateOfferAsync(id, model, userId!, cancellationToken).ConfigureAwait(false);
                //memgoni kargi praktikaa am shemtxvevashi TempDatas gamokeneba rata
                //redirecti roca xdeba or requests shoris shevinaxo es monacemi
                TempData["SuccessMessage"] = "Offer updated successfully!";
                return RedirectToAction("MyOffers");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating offer: " + ex.Message;
                return RedirectToAction("MyOffers");
            }

        }
        // POST: /Merchant/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _offerService.DeleteOfferAsync(id, userId!, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Offer deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MyOffers");

        }
        // POST: /Merchant/RedeemCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemCoupon(string code, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _couponService.RedeemCouponAsync(code, userId!, cancellationToken).ConfigureAwait(false);

                TempData["SuccessMessage"] = "Coupon validated and redeemed successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> ExportSalesToCsv(CancellationToken cancellationToken)
        {
            var merchantId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(merchantId))
            {
                return Unauthorized();
            }

            var salesHistory = await _offerService.GetMerchantSalesHistoryAsync(merchantId, cancellationToken);

            var builder = new StringBuilder();
            builder.AppendLine("Offer Title,Customer Name,Customer Email,Coupon Code,Purchase Date,Price");

            foreach (var sale in salesHistory)
            {
                var safeTitle = sale.OfferTitle?.Replace(",", "") ?? "Unknown Offer";
                var safeCustomerName = sale.CustomerName?.Replace(",", "") ?? "Unknown Customer";
                var safeEmail = sale.CustomerEmail?.Replace(",", "") ?? "No Email";

                var purchaseDate = sale.PurchasedAt.ToString("yyyy-MM-dd HH:mm");

                builder.AppendLine($"{safeTitle},{safeCustomerName},{safeEmail},{sale.CouponCode},{purchaseDate},{sale.Price}");
            }

            var fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var fileName = $"SalesHistory_{DateTime.Now:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }

    }
}
