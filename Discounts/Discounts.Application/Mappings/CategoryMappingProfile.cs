// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Category;
using Discounts.Domain.Entities;

namespace Discounts.Application.Mappings
{
    //automappers categoriebis shesaxeb vutxrat unda
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
        }
    }
}
