// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Category;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: categories/Index
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
            return View(categories);
        }

        // GET:  /Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            await _categoryService.CreateCategoryAsync(model, cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Category deleted successfully";
            }
            catch (Exception)
            {
                //if categ contains offer
                TempData["ErrorMessage"] = "Cannot delete - This category contains active offers";
            }
            return RedirectToAction(nameof(Index));
        }
        // GET /Categories/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
            var category = categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateCategoryDto
            {
                Name = category.Name
            };

            ViewBag.CategoryId = id;
            return View(updateDto);
        }

        //POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(model);
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, model, cancellationToken).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Category updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category");
                ViewBag.CategoryId = id;
                return View(model);
            }
        }
    }
}
