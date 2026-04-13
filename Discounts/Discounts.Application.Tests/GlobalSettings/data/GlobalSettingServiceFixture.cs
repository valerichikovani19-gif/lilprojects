using Discounts.Application.RepoInterfaces;
using Discounts.Application.Services;
using Moq;
namespace Discounts.Application.Tests.GlobalSettings.Data
{
    public class GlobalSettingServiceFixture : IDisposable
    {
        public GlobalSettingService Service { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IGlobalSettingRepository> SettingsRepoMock { get; }

        public GlobalSettingServiceFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            SettingsRepoMock = new Mock<IGlobalSettingRepository>();

            UnitOfWorkMock.Setup(u => u.GlobalSettings).Returns(SettingsRepoMock.Object);

            Service = new GlobalSettingService(UnitOfWorkMock.Object);
        }

        public void Dispose()
        {
        }
    }
}
