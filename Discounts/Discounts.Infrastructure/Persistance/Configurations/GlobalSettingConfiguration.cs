// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Infrastructure.Persistance.Configurations
{
    public class GlobalSettingConfiguration : IEntityTypeConfiguration<GlobalSetting>
    {
        public void Configure(EntityTypeBuilder<GlobalSetting> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasData(new GlobalSetting
            {
                Id = 1,
                ReservationTimeInMinutes = 30,
                MerchantEditWindowInHours = 24
            });
        }
    }
}
