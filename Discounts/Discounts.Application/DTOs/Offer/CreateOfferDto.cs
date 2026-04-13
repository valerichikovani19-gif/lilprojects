// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Offer
{
    public class CreateOfferDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public decimal OriginalPrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public int TotalQuantity { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        public int CategoryId { get; set; }
    }
}
