// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Merchant
{
    public class MerchantDashboardDto
    {
        public int ActiveOffersCount { get; set; }
        public int ExpiredOffersCount { get; set; }
        public int TotalCouponsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
