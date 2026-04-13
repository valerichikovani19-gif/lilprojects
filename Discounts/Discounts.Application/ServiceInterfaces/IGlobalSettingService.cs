// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;

namespace Discounts.Application.ServiceInterfaces
{
    public interface IGlobalSettingService
    {
        Task<GlobalSettingDto> GetSettingsAsync(CancellationToken cancellationToken);
        Task UpdateSettingsAsync(GlobalSettingDto settingsDto, CancellationToken cancellationToken);
    }
}
