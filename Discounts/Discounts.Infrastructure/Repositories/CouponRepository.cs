// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Discounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Coupon>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(x => x.CustomerId == customerId && !x.IsDeleted)
                .Include(x => x.Offer)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<List<Coupon>> FindExpiredReservationsAsync(DateTime cutoffDate, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(c => c.Status == CouponStatus.Reserved && c.ReservedAt < cutoffDate)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<List<Coupon>> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(x => x.Offer)
                .Where(x => x.Offer.MerchantId == merchantId && x.Status == CouponStatus.Purchased)
                .OrderByDescending(x => x.PurchasedAt)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<Coupon?> GetReservationAsync(int offerId, string customerId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c =>
                    c.OfferId == offerId &&
                    c.CustomerId == customerId &&
                    c.Status == CouponStatus.Reserved &&
                    !c.IsDeleted,
                    cancellationToken).ConfigureAwait(false);
        }

        public void Remove(Coupon coupon)
        {
            _dbSet.Remove(coupon);
        }
    }
}
