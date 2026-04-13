// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Infrastructure.Persistence.Configurations
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(1121);

            builder.Property(x => x.OriginalPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DiscountPrice).HasColumnType("decimal(18,2)");

            builder.Property(x => x.ImageUrl)
                .IsRequired(false);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Offers)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Merchant)
                .WithMany()
                .HasForeignKey(x => x.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
