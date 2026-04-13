using AutoMapper;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.Services;
using Moq;
namespace Discounts.Application.Tests.Coupons.Data
{
    public class CouponServiceFixture : IDisposable
    {
        public CouponService Service { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IOfferRepository> OfferRepoMock { get; }
        public Mock<ICouponRepository> CouponRepoMock { get; }
        public Mock<IMapper> MapperMock { get; }

        public CouponServiceFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            OfferRepoMock = new Mock<IOfferRepository>();
            CouponRepoMock = new Mock<ICouponRepository>();
            MapperMock = new Mock<IMapper>();

            UnitOfWorkMock.Setup(u => u.Offers).Returns(OfferRepoMock.Object);
            UnitOfWorkMock.Setup(u => u.Coupons).Returns(CouponRepoMock.Object);

            Service = new CouponService(UnitOfWorkMock.Object, MapperMock.Object);
        }

        public void Dispose()
        {
        }
    }
}
