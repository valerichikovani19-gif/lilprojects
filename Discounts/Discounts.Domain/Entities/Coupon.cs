// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Enums;

namespace Discounts.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        //unikaluri kodi titoeuli oferistvis merchantis sachveneblad(shemdeg redeem0
        public string Code { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        public CouponStatus Status { get; set; } = CouponStatus.Reserved;

        public DateTime? ReservedAt { get; set; }
        public DateTime? PurchasedAt { get; set; }

        // Foreign Keys
        public string CustomerId { get; set; } = string.Empty;

        public int OfferId { get; set; }
        public Offer Offer { get; set; } = null!;
    }
}
