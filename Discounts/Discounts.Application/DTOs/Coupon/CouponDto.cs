// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Coupon
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PurchasedAt { get; set; }

        public string OfferTitle { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
        public int OfferId { get; set; }
    }
}
