using AutoMapper;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Discounts.Application.Tests.Offers.Data
{
    public class OfferServiceFixture : IDisposable
    {
        public OfferService Service { get; private set; }

        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IOfferRepository> OfferRepoMock { get; }
        public Mock<IGlobalSettingRepository> SettingsRepoMock { get; }
        public Mock<ICategoryRepository> CategoryRepoMock { get; }
        public Mock<ICouponRepository> CouponRepoMock { get; }
        public Mock<IAuthRepository> AuthRepoMock { get; }
        public Mock<IMapper> MapperMock { get; }
        public Mock<IMemoryCache> CacheMock { get; }

        public OfferServiceFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            OfferRepoMock = new Mock<IOfferRepository>();
            SettingsRepoMock = new Mock<IGlobalSettingRepository>();
            CategoryRepoMock = new Mock<ICategoryRepository>();
            CouponRepoMock = new Mock<ICouponRepository>();
            AuthRepoMock = new Mock<IAuthRepository>();

            MapperMock = new Mock<IMapper>();
            CacheMock = new Mock<IMemoryCache>();

            UnitOfWorkMock.Setup(u => u.Offers).Returns(OfferRepoMock.Object);
            UnitOfWorkMock.Setup(u => u.GlobalSettings).Returns(SettingsRepoMock.Object);
            UnitOfWorkMock.Setup(u => u.Categories).Returns(CategoryRepoMock.Object);
            UnitOfWorkMock.Setup(u => u.Auth).Returns(AuthRepoMock.Object);
            UnitOfWorkMock.Setup(u => u.Coupons).Returns(CouponRepoMock.Object);

            Service = new OfferService(UnitOfWorkMock.Object, MapperMock.Object, CacheMock.Object);
        }

        public void Dispose()
        {
        }
    }
}
