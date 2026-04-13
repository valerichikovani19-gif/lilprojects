// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;
using Discounts.Application.RepoInterfaces;
using Discounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class GlobalSettingRepository : IGlobalSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public GlobalSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GlobalSetting> GetAsync(CancellationToken cancellationToken)
        {
            var settings = await _context.GlobalSettings.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return settings ?? new GlobalSetting
            {
                MerchantEditWindowInHours = 24,
                ReservationTimeInMinutes = 30
            };
        }
        public void Update(GlobalSetting setting)
        {
            _context.GlobalSettings.Update(setting);
        }
    }
}
