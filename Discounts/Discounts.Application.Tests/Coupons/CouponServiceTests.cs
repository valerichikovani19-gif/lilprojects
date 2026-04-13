using Discounts.Application.DTOs.Coupon;
using Discounts.Application.Exceptions;
using Discounts.Application.Tests.Coupons.Data;
using Discounts.Domain.Entities;
using Discounts.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discounts.Application.Tests.Coupons
{
    public class CouponServceTests : IClassFixture<CouponServiceFixture>
    {
        private readonly CouponServiceFixture _fixture;
        public CouponServceTests(CouponServiceFixture fixture)
        {
            _fixture = fixture;
            _fixture.OfferRepoMock.Reset();
            _fixture.CouponRepoMock.Reset();
            _fixture.MapperMock.Reset();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }
        #region Purchase tests
        [Fact(DisplayName = "Purchase - Should decrease quantity and create coupon when valid")]
        public async Task Purchase_WhenValid_ShouldSuccess()
        {
            int offerId = 1;
            string customerId = "user123";
            var offer = new Offer
            {
                Id = offerId,
                Status = OfferStatus.Active,
                AvailableQuantity = 10,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                DiscountPrice = 20,
                Title = "Good Deal"
            };
            //setup Repo
            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // setup mapper 
            _fixture.MapperMock.Setup(m => m.Map<CouponDto>(It.IsAny<Coupon>()))
                .Returns(new CouponDto { Code = "ABC-123", Status = "Purchased" });

            //act
            var result = await _fixture.Service.PurchaseCouponAsync(offerId, customerId, CancellationToken.None);

            // assert
            using (new AssertionScope())
            {
                offer.AvailableQuantity.Should().Be(9);

                _fixture.OfferRepoMock.Verify(x => x.Update(offer), Times.Once);

                // was coupon added to db
                _fixture.CouponRepoMock.Verify(x => x.AddAsync(
                    It.Is<Coupon>(c => c.Status == CouponStatus.Purchased && c.CustomerId == customerId),
                    It.IsAny<CancellationToken>()), Times.Once);
            }
        }
        [Fact(DisplayName = "Purchase - Should throw StockDepletedException when Quantity is 0")]
        public async Task Purchase_WhenNoStock_ShouldThrowException()
        {
            //arrange
            var offer = new Offer
            {
                Id = 1,
                Status = OfferStatus.Active,
                AvailableQuantity = 0,
                ValidUntil = DateTime.UtcNow.AddDays(5)
            };

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            //Act
            var action = async () => await _fixture.Service.PurchaseCouponAsync(1, "user-1", CancellationToken.None);

            //assert
            await action.Should().ThrowAsync<StockDepletedException>();
        }

        [Fact(DisplayName = "Purchase - Should throw OfferNotActiveException when expired")]
        public async Task Purchase_WhenExpired_ShouldThrowException()
        {
            // arrange
            var offer = new Offer
            {
                Id = 1,
                Status = OfferStatus.Active,
                AvailableQuantity = 10,
                ValidUntil = DateTime.UtcNow.AddDays(-1) //expired
            };

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            //act
            var action = async () => await _fixture.Service.PurchaseCouponAsync(1, "user-1", CancellationToken.None);

            //assert
            await action.Should().ThrowAsync<OfferNotActiveException>();
        }
        #endregion
        #region reserve
        [Fact(DisplayName = "Reserve - Should create Coupon with Reserved status")]
        public async Task Reserve_WhenValid_ShouldSetStatusReserved()
        {
            // arrange
            int offerId = 1;
            var offer = new Offer
            {
                Id = offerId,
                Status = OfferStatus.Active,
                AvailableQuantity = 5,
                ValidUntil = DateTime.UtcNow.AddDays(5),
                Title = "Reserved Deal"
            };

            _fixture.OfferRepoMock.Setup(x => x.GetByIdAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            _fixture.MapperMock.Setup(m => m.Map<CouponDto>(It.IsAny<Coupon>()))
                .Returns(new CouponDto { Code = "RES-123", Status = "Reserved" });

            //act
            await _fixture.Service.ReserveCouponAsync(offerId, "user-1", CancellationToken.None);

            //assert
            _fixture.CouponRepoMock.Verify(x => x.AddAsync(
                It.Is<Coupon>(c => c.Status == CouponStatus.Reserved),
                It.IsAny<CancellationToken>()), Times.Once);

            offer.AvailableQuantity.Should().Be(4);
        }
        #endregion
    }
}
