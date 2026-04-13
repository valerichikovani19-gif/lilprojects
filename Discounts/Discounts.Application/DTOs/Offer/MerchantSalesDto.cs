// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Offer
{
    public class MerchantSalesDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public string OfferTitle { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty; //fn + ln

        public DateTime PurchasedAt { get; set; }
        public decimal Price { get; set; }
    }
}
