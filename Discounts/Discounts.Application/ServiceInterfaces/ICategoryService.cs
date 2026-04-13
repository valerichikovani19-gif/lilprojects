// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Category;

namespace Discounts.Application.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken);
        Task<int> CreateCategoryAsync(CreateCategoryDto categoryDto, CancellationToken cancellationToken);
        // adding upd/del when admin needs it
        Task UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto, CancellationToken cancellationToken);
        Task DeleteCategoryAsync(int id, CancellationToken cancellationToken);
    }
}
