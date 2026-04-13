// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Offer;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;

namespace Discounts.Application.Mappings
{
    public class OfferMappingProfile : Profile
    {
        public OfferMappingProfile()
        {
            //entity - dto
            CreateMap<Offer, OfferDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            //createDto - entity
            CreateMap<CreateOfferDto, Offer>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OfferStatus.Pending))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.TotalQuantity));

            //UpdateDto - entity
            CreateMap<UpdateOfferDto, Offer>();
        }
    }
}
