// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Category;
using Discounts.Application.Exceptions;
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Application.ServiceInterfaces;

namespace Discounts.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region crud
        public async Task<int> CreateCategoryAsync(CreateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            //var exists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == categoryDto.Name,cancellationToken);
            //if (exists) throw new BadRequestException($"Category '{categoryDto.Name}' already exists");
            var category = _mapper.Map<Category>(categoryDto);
            await _unitOfWork.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return category.Id;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<CategoryDto>>(categories);
        }
        public async Task UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (category == null)
                throw new CategoryNotFoundException(id);
            category.Name = categoryDto.Name;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (category == null)
                throw new CategoryNotFoundException(id);
            var hasAttachedOffers = await _unitOfWork.Offers.ExistsAsync(offer => offer.CategoryId == id, cancellationToken).ConfigureAwait(false);

            if (hasAttachedOffers)
            {
                throw new InvalidOperationException("Cannot delete - This category contains active offers");
            }
            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}
