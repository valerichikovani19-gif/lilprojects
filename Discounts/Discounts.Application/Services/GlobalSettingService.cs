// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.ServiceInterfaces;
namespace Discounts.Application.Services
{
    public class GlobalSettingService : IGlobalSettingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GlobalSettingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GlobalSettingDto> GetSettingsAsync(CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken).ConfigureAwait(false);
            return new GlobalSettingDto
            {
                ReservationTimeInMinutes = settings.ReservationTimeInMinutes,
                MerchantEditWindowInHours = settings.MerchantEditWindowInHours
            };
        }

        public async Task UpdateSettingsAsync(GlobalSettingDto settingsDto, CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken).ConfigureAwait(false);

            //upd fields
            settings.ReservationTimeInMinutes = settingsDto.ReservationTimeInMinutes;
            settings.MerchantEditWindowInHours = settingsDto.MerchantEditWindowInHours;

            _unitOfWork.GlobalSettings.Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
