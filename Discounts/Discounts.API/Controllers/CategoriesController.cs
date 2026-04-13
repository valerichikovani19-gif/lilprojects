// Copyright (C) TBC Bank. All Rights Reserved.
using Asp.Versioning;
using Discounts.Application.DTOs.Category;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Discounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        /// <summary>
        /// Gets all available categories. Accessible by everyone
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of categories</returns>
        /// <response code="200">Returns the list of categories</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);
            return Ok(categories);
        }
        /// <summary>
        /// Creates a new category. Only Admin can do this
        /// </summary>
        /// <param name="categoryDto">Category creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The ID of the newly created category</returns>
        /// <response code="200">Category created successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var id = await _categoryService.CreateCategoryAsync(categoryDto, cancellationToken).ConfigureAwait(false);
            return Ok(new { Id = id });
        }
        /// <summary>
        /// Updates an existing category. Only Admin can do this
        /// </summary>
        /// <param name="id">The ID of the category to update</param>
        /// <param name="categoryDto">The updated category details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content.</returns>
        /// <response code="204">Category updated successfully</response>
        /// <response code="404">Category not found</response>
        /// <response code="403">Forbidden</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            await _categoryService.UpdateCategoryAsync(id, categoryDto, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        /// <summary>
        /// Deletes a category (soft delete). Only Admin can do this
        /// </summary>
        /// <param name="id">The ID of the category to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        /// <response code="204">Category deleted successfully</response>
        /// <response code="404">Category not found</response>
        /// <response code="403">Forbidden</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _categoryService.DeleteCategoryAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
