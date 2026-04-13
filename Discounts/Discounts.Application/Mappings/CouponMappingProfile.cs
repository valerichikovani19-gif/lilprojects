// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Coupon;
using Discounts.Domain.Entities;

namespace Discounts.Application.Mappings
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {

            CreateMap<Coupon, CouponDto>().ForMember(dest => dest.OfferTitle, opt => opt.MapFrom(src => src.Offer.Title))
                .ForMember(dest => dest.PricePaid, opt => opt.MapFrom(src => src.Offer.DiscountPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OfferId, opt => opt.MapFrom(src => src.OfferId));
        }
    }
}
