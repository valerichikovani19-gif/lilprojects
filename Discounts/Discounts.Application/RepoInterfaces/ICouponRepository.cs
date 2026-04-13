// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;

namespace Discounts.Application.RepoInterfaces
{
    public interface ICouponRepository : IGenericRepository<Coupon>
    {
        Task<List<Coupon>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken);
        Task<List<Coupon>> FindExpiredReservationsAsync(DateTime cutoffDate, CancellationToken cancellationToken);
        Task<List<Coupon>> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken);

        Task<Coupon?> GetReservationAsync(int offerId, string customerId, CancellationToken cancellationToken);
        void Remove(Coupon coupon);
    }
}
