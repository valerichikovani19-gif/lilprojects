// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Coupon;

namespace Discounts.Application.ServiceInterfaces
{
    public interface ICouponService
    {
        //dasajavshnad
        Task<CouponDto> ReserveCouponAsync(int offerId, string customerId, CancellationToken cancellationToken);

        Task<CouponDto> PurchaseCouponAsync(int offerId, string customerId, CancellationToken cancellationToken);
        Task<List<CouponDto>> GetMyCouponsAsync(string customerId, CancellationToken cancellationToken);
        Task RedeemCouponAsync(string code, string merchantId, CancellationToken cancellationToken);
    }
}
