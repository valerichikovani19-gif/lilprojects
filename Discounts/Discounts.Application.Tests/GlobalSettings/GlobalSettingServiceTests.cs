using Discounts.Application.DTOs.Admin;
using Discounts.Application.Tests.GlobalSettings.Data;
using Discounts.Domain.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Discounts.Application.Tests.GlobalSettings
{
    public class GlobalSettingServiceTests : IClassFixture<GlobalSettingServiceFixture>
    {
        private readonly GlobalSettingServiceFixture _fixture;

        public GlobalSettingServiceTests(GlobalSettingServiceFixture fixture)
        {
            _fixture = fixture;
            _fixture.SettingsRepoMock.Reset();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "GetSettings - Should return mapped DTO values")]
        public async Task GetSettings_ShouldReturnDto()
        {
            // arrange
            var entity = new GlobalSetting
            {
                ReservationTimeInMinutes = 30,
                MerchantEditWindowInHours = 24
            };

            _fixture.SettingsRepoMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            //act
            var result = await _fixture.Service.GetSettingsAsync(CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                result.ReservationTimeInMinutes.Should().Be(30);
                result.MerchantEditWindowInHours.Should().Be(24);

                // was repo called
                _fixture.SettingsRepoMock.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact(DisplayName = "UpdateSettings - Should update entity properties and save changes")]
        public async Task UpdateSettings_ShouldUpdateAndSave()
        {
            // arrange
            var existingEntity = new GlobalSetting
            {
                ReservationTimeInMinutes = 30,
                MerchantEditWindowInHours = 24
            };

            var updateDto = new GlobalSettingDto
            {
                ReservationTimeInMinutes = 60,
                MerchantEditWindowInHours = 48
            };

            _fixture.SettingsRepoMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEntity);

            //act
            await _fixture.Service.UpdateSettingsAsync(updateDto, CancellationToken.None);

            //Assert
            using (new AssertionScope())
            {
                // was entity in memory changed
                existingEntity.ReservationTimeInMinutes.Should().Be(60);
                existingEntity.MerchantEditWindowInHours.Should().Be(48);

                // verifying if update was called on repo
                _fixture.SettingsRepoMock.Verify(x => x.Update(existingEntity), Times.Once);

                //verifying SaveChanges call
                _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
