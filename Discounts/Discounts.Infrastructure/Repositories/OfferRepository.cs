// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Discounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Offer>> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(x => x.MerchantId == merchantId && !x.IsDeleted)
                .Include(x => x.Category)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Offer>> GetAllActiveAsync(CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(x => x.Status == OfferStatus.Active && !x.IsDeleted && x.ValidUntil > DateTime.UtcNow)
                .Include(x => x.Category)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<List<Offer>> GetPendingOffersAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.Where(x => x.Status == OfferStatus.Pending && !x.IsDeleted)
                .Include(x => x.Category)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task UpdateExpirationStatusAsync(CancellationToken cancellationToken)
        {
            var expiredOffers = await _dbSet.Where(x => x.Status == OfferStatus.Active && x.ValidUntil < DateTime.UtcNow)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            if (expiredOffers.Any())
            {
                foreach (var offer in expiredOffers)
                {
                    offer.Status = OfferStatus.Expired;
                }
                //savechanges akvizamdi magram gamomdzaxebelic gaaketebs isedac UnitOfWorkidn
            }
        }

        public async Task<List<Offer>> GetAllActiveAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken)
        {
            var query = _dbSet
         .Where(x => x.Status == OfferStatus.Active && !x.IsDeleted && x.ValidUntil > DateTime.UtcNow)
         .Include(x => x.Category)
         .AsQueryable();

            //filter by title or descr
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
            }

            //filter by categ
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            //filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(x => x.DiscountPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.DiscountPrice <= maxPrice.Value);
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<(List<Offer> Offers, int TotalCount)> GetAllActiveAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _dbSet
                .Where(x => x.Status == OfferStatus.Active && !x.IsDeleted && x.ValidUntil > DateTime.UtcNow)
                .Include(x => x.Category)
                .AsQueryable();

            //same filts
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(x => x.DiscountPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.DiscountPrice <= maxPrice.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            //applying pagination
            var items = await query
                .OrderByDescending(x => x.CreatedAt)//sort
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return (items, totalCount);
        }
    }
}
