// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Persistence;

namespace Discounts.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
