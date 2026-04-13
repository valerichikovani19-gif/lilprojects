// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;

namespace Discounts.Application.RepoInterfaces
{
    public interface IGlobalSettingRepository
    {
        Task<GlobalSetting> GetAsync(CancellationToken cancellationToken);
        void Update(GlobalSetting setting);
    }
}
