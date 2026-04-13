using AutoMapper;
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Offer;
using Discounts.Application.Exceptions;
using Discounts.Application.Services;
using Discounts.Application.Tests.Offers.Data;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System.Linq.Expressions;

namespace Discounts.Application.Tests.Offers
{
    public class OfferServiceTests : IClassFixture<OfferServiceFixture>
    {
        public readonly OfferServiceFixture _fixture;
        public OfferServiceTests(OfferServiceFixture fixture)
        {
            _fixture = fixture;
            //fresh stateistvis mockebis dareseteba;
            _fixture.OfferRepoMock.Reset();
            _fixture.SettingsRepoMock.Reset();
            _fixture.CategoryRepoMock.Reset();
            _fixture.MapperMock.Reset();
            _fixture.CouponRepoMock.Reset();
            _fixture.AuthRepoMock.Reset();
        }
        #region create offer test
        [Fact(DisplayName = "CreateOffer -Should return new Id when category exists")]
        public async Task CreateOffer_WhenValid_ShouldReturnId()
        {
            var createDto = new CreateOfferDto { CategoryId = 1, Title = "New offer" };
            var offerEntity = new Offer { Id = 101, Title = "New offer" };
            _fixture.CategoryRepoMock.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _fixture.MapperMock.Setup(m => m.Map<Offer>(createDto)).Returns(offerEntity);
            //act
            var resultId = await _fixture.Service.CreateOfferAsync(createDto, "merchant1", CancellationToken.None);
            //assert
            _fixture.OfferRepoMock.Verify(x => x.AddAsync(offerEntity, It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion
        #region update tests
        [Fact(DisplayName = "UpdateOffer - Should throw UnauthorizedAccessException when user is not the Owner")]
        public async Task UpdateOffer_WhenNotOwner_ShouldThrowUnauthorized()
        {
            //arrange
            var offerId = 1;
            var existingOffer = new Offer { Id = offerId, MerchantId = "owner-merchant" };
            var attackerId = "hacker-merchant";

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingOffer);

            // act
            var action = async () => await _fixture.Service.UpdateOfferAsync(offerId, new UpdateOfferDto(), attackerId, CancellationToken.None);

            //assert
            await action.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*permission*");
        }
        [Fact(DisplayName = "UpdateOffer - Should update fields when User is Owner and Time is valid")]
        public async Task UpdateOffer_WhenValid_ShouldUpdateAndSave()
        {
            //arrange
            var offerId = 1;
            var merchantId = "my-merchant";
            var existingOffer = new Offer
            {
                Id = offerId,
                MerchantId = merchantId,
                CreatedAt = DateTime.UtcNow
            };

            _fixture.SettingsRepoMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlobalSetting { MerchantEditWindowInHours = 24 });

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingOffer);

            await _fixture.Service.UpdateOfferAsync(offerId, new UpdateOfferDto(), merchantId, CancellationToken.None);

            //assert
            _fixture.OfferRepoMock.Verify(x => x.Update(existingOffer), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion
        //admin tests
        [Fact(DisplayName = "ApproveOffer: Should change status to Active")]
        public async Task ApproveOffer_ShouldSetStatusActive()
        {
            //arrange
            var offer = new Offer { Id = 1, Status = OfferStatus.Pending };
            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            //act
            await _fixture.Service.ApproveOfferAsync(1, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                offer.Status.Should().Be(OfferStatus.Active);
                offer.RejectionReason.Should().BeNull();
                _fixture.OfferRepoMock.Verify(x => x.Update(offer), Times.Once);
            }
        }
        [Fact(DisplayName = "RejectOffer - Should change status to Rejected and set Reason")]
        public async Task RejectOffer_ShouldSetStatusRejected()
        {
            //arrange
            var offer = new Offer { Id = 1, Status = OfferStatus.Pending };
            var rejectDto = new RejectOfferDto { Reason = "Bad Image" };

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // act
            await _fixture.Service.RejectOfferAsync(1, rejectDto, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                offer.Status.Should().Be(OfferStatus.Rejected);
                offer.RejectionReason.Should().Be("Bad Image");
                _fixture.OfferRepoMock.Verify(x => x.Update(offer), Times.Once);
            }
        }
        //dashboard tests
        [Fact(DisplayName = "GetDashboard - Should correctly calculate Active, Expired, Sold, and Revenue stats")]
        public async Task GetDashboard_ShouldCalculateStats()
        {
            // arrange
            var merchantId = "merchant-1";
            var offers = new List<Offer>
            {
                new Offer {
                    Status = OfferStatus.Active,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                    TotalQuantity = 100,
                    AvailableQuantity = 90,
                    DiscountPrice = 50
                },
                
                new Offer {
                    Status = OfferStatus.Expired,
                    ValidUntil = DateTime.UtcNow.AddDays(-1),
                    TotalQuantity = 50,
                    AvailableQuantity = 45,
                    DiscountPrice = 20
                }
            };
            var soldCoupons = new List<Coupon>
            {
                new Coupon { Offer = new Offer { DiscountPrice = 50 } },
                new Coupon { Offer = new Offer { DiscountPrice = 50 } },
                new Coupon { Offer = new Offer { DiscountPrice = 20 } }
            };
            //setup
            _fixture.OfferRepoMock.Setup(x => x.GetByMerchantIdAsync(merchantId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(offers);
            _fixture.CouponRepoMock.Setup(x => x.GetByMerchantIdAsync(merchantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(soldCoupons);

            //act
            var result = await _fixture.Service.GetMerchantDashboardAsync(merchantId, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                result.ActiveOffersCount.Should().Be(1);//one active
                result.ExpiredOffersCount.Should().Be(1);//one sold

                result.TotalCouponsSold.Should().Be(3); //3 sold coups
                result.TotalRevenue.Should().Be(120m);//rev sh be 120
            }
        }

    }
}
