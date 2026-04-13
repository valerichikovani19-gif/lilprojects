// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.DTOs.Offer
{
    public class OfferDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public decimal OriginalPrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public int AvailableQuantity { get; set; }
        public int TotalQuantity { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        public string Status { get; set; } = string.Empty; //enum as str

        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
