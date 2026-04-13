// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Common;
using Discounts.Application.DTOs.Merchant;
using Discounts.Application.DTOs.Offer;
using Discounts.Application.Exceptions;
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Discounts.Application.ServiceInterfaces;
using Microsoft.Extensions.Caching.Memory;
namespace Discounts.Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public OfferService(IUnitOfWork unitOfWork, IMapper mapper,IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<int> CreateOfferAsync(CreateOfferDto offerDto, string merchantId, CancellationToken cancellationToken)
        {
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == offerDto.CategoryId, cancellationToken).ConfigureAwait(false);
            if (!categoryExists)
                throw new CategoryNotFoundException(offerDto.CategoryId);
        
            var offer = _mapper.Map<Offer>(offerDto);
            offer.MerchantId = merchantId;
            

            await _unitOfWork.Offers.AddAsync(offer, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return offer.Id;
        }

        public async Task<OfferDto> GetOfferByIdAsync(int id, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (offer == null)
                throw new OfferNotFoundException(id);

            return _mapper.Map<OfferDto>(offer);
        }

        public async Task<List<OfferDto>> GetMerchantOffersAsync(string merchantId, CancellationToken cancellationToken)
        {
            var offers = await _unitOfWork.Offers.GetByMerchantIdAsync(merchantId, cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<OfferDto>>(offers);
        }

        public async Task UpdateOfferAsync(int id, UpdateOfferDto offerDto, string merchantId, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (offer == null)
                throw new OfferNotFoundException(id);

            if (offer.MerchantId != merchantId)
                throw new UnauthorizedAccessException("Nope you do not have permission to edit this offer");
            
            var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken).ConfigureAwait(false);
            var deadline = offer.CreatedAt.AddHours(settings.MerchantEditWindowInHours);
            if (DateTime.UtcNow > deadline)
            {
                throw new UnauthorizedActionException(
                    $"No you can no longer edit this offer.edit window is closed");
            }
            _mapper.Map(offerDto, offer);
            //if offer was rejected,reseting to pending so admin sees it again
            if (offer.Status == OfferStatus.Rejected)
            {
                offer.Status = OfferStatus.Pending;
                offer.RejectionReason = null;
            }

            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<OfferDto>> GetPendingOffersAsync(CancellationToken cancellationToken)
        {
            var offers = await _unitOfWork.Offers.GetPendingOffersAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<OfferDto>>(offers);
        }
        public async Task ApproveOfferAsync(int id, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (offer == null) throw new OfferNotFoundException(id);

            offer.Status = OfferStatus.Active;
            offer.RejectionReason = null;
            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task RejectOfferAsync(int id, RejectOfferDto rejectDto, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (offer == null) throw new OfferNotFoundException(id);

            offer.Status = OfferStatus.Rejected;
            offer.RejectionReason = rejectDto.Reason;

            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<OfferDto>> GetActiveOffersAsync(CancellationToken cancellationToken)
        {
            var offers = await _unitOfWork.Offers.GetAllActiveAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<OfferDto>>(offers);
        }

        public async Task<List<MerchantSalesDto>> GetMerchantSalesHistoryAsync(string merchantId, CancellationToken cancellationToken)
        {
            var coupons = await _unitOfWork.Coupons.GetByMerchantIdAsync(merchantId, cancellationToken).ConfigureAwait(false);
            if (!coupons.Any())
                return new List<MerchantSalesDto>();

            //getting unique userids from coups
            var customerIds = coupons.Select(c => c.CustomerId).Distinct().ToList();
            //getting details from AuthRep
            var customers = await _unitOfWork.Auth.GetUsersByIdsAsync(customerIds, cancellationToken).ConfigureAwait(false);
            var salesHistory = new List<MerchantSalesDto>();

            foreach (var coupon in coupons)
            {
                var customer = customers.FirstOrDefault(u => u.Id == coupon.CustomerId);

                salesHistory.Add(new MerchantSalesDto
                {
                    CouponCode = coupon.Code,
                    OfferTitle = coupon.Offer.Title,
                    PurchasedAt = coupon.PurchasedAt ?? DateTime.MinValue,
                    Price = coupon.Offer.DiscountPrice,
                    //
                    CustomerEmail = customer?.Email ?? "Unknown",
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown User"
                });
            }

            return salesHistory;
        }
        public async Task<List<OfferDto>> GetActiveOffersAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken)
        {
            var offers = await _unitOfWork.Offers.GetAllActiveAsync(search, categoryId, minPrice, maxPrice, cancellationToken).ConfigureAwait(false);

            return _mapper.Map<List<OfferDto>>(offers);
        }
        //cachirebas vizam ak cotaxanshi 
        public async Task<PagedResponse<OfferDto>> GetActiveOffersAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var cacheKey = $"ActiveOffers_{search}_{categoryId}_{minPrice}_{maxPrice}_{pageIndex}_{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out PagedResponse<OfferDto>? pagedResponse))
            {
                //tu ramshi araris monacemi chveulebisamebr bazas miakitxavs
                var (offers, totalCount) = await _unitOfWork.Offers.GetAllActiveAsync(
                    search, categoryId, minPrice, maxPrice, pageIndex, pageSize, cancellationToken).ConfigureAwait(false);

                var offerDtos = _mapper.Map<List<OfferDto>>(offers);
                //response objecti
                pagedResponse = new PagedResponse<OfferDto>(offerDtos, totalCount, pageIndex, pageSize);
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(cacheKey,pagedResponse,cacheOptions);
            }
            return pagedResponse!;
        }
        public async Task<MerchantDashboardDto> GetMerchantDashboardAsync(string merchantId, CancellationToken cancellationToken)
        {
            var offers = await _unitOfWork.Offers.GetByMerchantIdAsync(merchantId, cancellationToken).ConfigureAwait(false);
             var soldCoupons = await _unitOfWork.Coupons.GetByMerchantIdAsync(merchantId, cancellationToken).ConfigureAwait(false);

            var stats = new MerchantDashboardDto
            {
                ActiveOffersCount = offers.Count(o => o.Status == OfferStatus.Active && o.ValidUntil > DateTime.UtcNow),

                ExpiredOffersCount = offers.Count(o => o.Status == OfferStatus.Expired || o.ValidUntil <= DateTime.UtcNow),

                TotalCouponsSold = soldCoupons.Count,

                TotalRevenue = soldCoupons.Sum(c => c.Offer.DiscountPrice)
            };

            return stats;
        }
        public async Task DeleteOfferAsync(int id, string merchantId, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (offer == null) throw new OfferNotFoundException(id);

            if (offer.MerchantId != merchantId)
                throw new UnauthorizedActionException("You can only delete your own offers.");

            //cant delete if t has sold coupons
            // if (offer.AvailableQuantity != offer.TotalQuantity) throw new InvalidOperationException("cannot delete offer with active sales");

            _unitOfWork.Offers.Delete(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task DeleteOfferByAdminAsync(int id, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (offer == null)
                throw new OfferNotFoundException(id);
            _unitOfWork.Offers.Delete(offer);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
