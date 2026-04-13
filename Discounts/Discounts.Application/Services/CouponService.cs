// Copyright (C) TBC Bank. All Rights Reserved.
using AutoMapper;
using Discounts.Application.DTOs.Coupon;
using Discounts.Application.Exceptions;
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using Discounts.Application.ServiceInterfaces;

namespace Discounts.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CouponService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CouponDto> PurchaseCouponAsync(int offerId, string customerId, CancellationToken cancellationToken)
        {
            var existingReservation = await _unitOfWork.Coupons.GetReservationAsync(offerId, customerId, cancellationToken).ConfigureAwait(false);

            if (existingReservation != null)
            {
                // reservaciis purchaseshi dakonvert
                // dekrementi reservshi

                existingReservation.Status = CouponStatus.Purchased;
                existingReservation.PurchasedAt = DateTime.UtcNow;

                _unitOfWork.Coupons.Update(existingReservation);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var dto = _mapper.Map<CouponDto>(existingReservation);
                // offer titleis dafetchva OfferDtostvis
                var offer = await _unitOfWork.Offers.GetByIdAsync(offerId, cancellationToken).ConfigureAwait(false);
                if (offer == null) throw new OfferNotFoundException(offerId);
                dto.OfferTitle = offer.Title;
                dto.PricePaid = offer.DiscountPrice;

                return dto;
            }
            else
            {
                //direct purchase
                var offer = await _unitOfWork.Offers.GetByIdAsync(offerId, cancellationToken).ConfigureAwait(false);
                if (offer == null) 
                    throw new OfferNotFoundException(offerId);

                if (offer.Status != OfferStatus.Active) 
                    throw new OfferNotActiveException();
                if (offer.ValidUntil < DateTime.UtcNow)
                    throw new OfferNotActiveException();
                if (offer.AvailableQuantity <= 0) 
                    throw new StockDepletedException(offer.Title);

                offer.AvailableQuantity -= 1;
                _unitOfWork.Offers.Update(offer);
                var coupon = new Coupon
                {
                    Code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Status = CouponStatus.Purchased,
                    PurchasedAt = DateTime.UtcNow,
                    CustomerId = customerId,
                    OfferId = offerId
                };

                await _unitOfWork.Coupons.AddAsync(coupon, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var dto = _mapper.Map<CouponDto>(coupon);
                dto.OfferTitle = offer.Title;
                dto.PricePaid = offer.DiscountPrice;

                return dto;
            }
        }

        public async Task<List<CouponDto>> GetMyCouponsAsync(string customerId, CancellationToken cancellationToken)
        {
            var coupons = await _unitOfWork.Coupons.GetByCustomerIdAsync(customerId, cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<CouponDto>>(coupons);
        }

        public async Task<CouponDto> ReserveCouponAsync(int offerId, string customerId, CancellationToken cancellationToken)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(offerId, cancellationToken).ConfigureAwait(false);
            if (offer == null) throw new OfferNotFoundException(offerId);
            if (offer.Status != OfferStatus.Active)
                throw new OfferNotActiveException();

            if (offer.ValidUntil < DateTime.UtcNow)
                throw new OfferNotActiveException();

            if (offer.AvailableQuantity <= 0)
                throw new StockDepletedException(offer.Title);
            //reservis dros iklebs raodenoba
            offer.AvailableQuantity -= 1;
            _unitOfWork.Offers.Update(offer);

            var coupon = new Coupon
            {
                Code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Status = CouponStatus.Reserved,
                ReservedAt = DateTime.UtcNow,   //worker swored am dros amowmebs
                CustomerId = customerId,
                OfferId = offerId
            };
            await _unitOfWork.Coupons.AddAsync(coupon, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // da bolos davabrunebt dtos
            var dto = _mapper.Map<CouponDto>(coupon);
            dto.OfferTitle = offer.Title;
            return dto;
        }
        public async Task RedeemCouponAsync(string code, string merchantId, CancellationToken cancellationToken)
        {
            //fetching all coups
            var allCoupons = await _unitOfWork.Coupons.GetAllAsync(cancellationToken).ConfigureAwait(false);

            var coupon = allCoupons.FirstOrDefault(c => c.Code == code);

            if (coupon == null)
            {
                throw new KeyNotFoundException($"Coupon with code - {code}-  not found");
            }
            //checking if this coupon belongs to the mecrhant
            var offer = await _unitOfWork.Offers.GetByIdAsync(coupon.OfferId, cancellationToken).ConfigureAwait(false);
            if (offer == null || offer.MerchantId != merchantId)
            {
                throw new UnauthorizedAccessException("this coupon does not belong to your offers");
            }
            if (coupon.Status == CouponStatus.Used)
            {
                throw new InvalidOperationException("This coupon has already been used");
            }

            if (coupon.Status != CouponStatus.Purchased)
            {
                throw new InvalidOperationException("coupon should be purchased first");
            }
            coupon.Status = CouponStatus.Used;
            _unitOfWork.Coupons.Update(coupon);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
