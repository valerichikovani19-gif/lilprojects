// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Enums;

namespace Discounts.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public decimal OriginalPrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        public string MerchantId { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public virtual ApplicationUser Merchant { get; set; } = null!;
        public string? RejectionReason { get; set; }
        
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    
    }
}
