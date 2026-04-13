// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;
using Discounts.Application.Exceptions;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
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
        //dashboard
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pendingOffers = await _offerService.GetPendingOffersAsync(cancellationToken).ConfigureAwait(false);
            return View(pendingOffers);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _offerService.ApproveOfferAsync(id, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Offer approved successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Reject(int id)
        {
            ViewBag.OfferId = id;
            return View(new RejectOfferDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, RejectOfferDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OfferId = id;
                return View(model);
            }
            try
            {
                await _offerService.RejectOfferAsync(id, model, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Offer rejected";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.OfferId = id;
                return View(model);
            }
        }
        //user managament
        [HttpGet]
        public async Task<IActionResult> Users(CancellationToken cancellationToken)
        {
            var users = await _authService.GetAllUsersAsync(cancellationToken).ConfigureAwait(false);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string email, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.BlockUserAsync(email, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"User {email} blocked";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error blocking user: " + ex.Message;
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string email, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.UnblockUserAsync(email, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"User {email} unblocked";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "error unblocking user: " + ex.Message;
            }
            return RedirectToAction("Users");
        }
        //global settings
        [HttpGet]
        public async Task<IActionResult> Settings(CancellationToken cancellationToken)
        {
            var settings = await _settingsService.GetSettingsAsync(cancellationToken).ConfigureAwait(false);
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(GlobalSettingDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _settingsService.UpdateSettingsAsync(model, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "settings updated successfully";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
        // Post /Admin/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _offerService.DeleteOfferByAdminAsync(id, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "0ffer deleted by Admin";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "error deleting offer: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
        // GEt /Admin/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new CreateUserDto());
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _authService.CreateUserByAdminAsync(model, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"User {model.Email} was successfully created and assigned the {model.Role} role.";
                return RedirectToAction("Users");
            }
            catch (UserOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET /Admin/EditUser?email=...
        [HttpGet]
        public async Task<IActionResult> EditUser(string email, CancellationToken cancellationToken)
        {
            var users = await _authService.GetAllUsersAsync(cancellationToken).ConfigureAwait(false);
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["ErrorMessage"] = "The requested user could not be found.";
                return RedirectToAction("Users");
            }

            var updateDto = new UpdateUserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            ViewBag.UserEmail = email;
            return View(updateDto);
        }

        // POst:/Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string email, UpdateUserDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserEmail = email;
                return View(model);
            }

            try
            {
                await _authService.UpdateUserAsync(email, model, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "The user's profile was successfully updated.";
                return RedirectToAction("Users");
            }
            catch (UserNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Users");
            }
            catch (UserOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.UserEmail = email;
                return View(model);
            }
        }

        // POST:/Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string email, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.DeleteUserAsync(email, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"The account for {email} has been permanently deleted.";
            }
            catch (UserNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UserOperationException ex)
            {
                TempData["ErrorMessage"] = "Deletion failed: " + ex.Message;
            }

            return RedirectToAction("Users");
        }
    }
}
