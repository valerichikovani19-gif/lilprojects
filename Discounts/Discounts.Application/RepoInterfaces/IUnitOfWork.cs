// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.RepoInterfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IOfferRepository Offers { get; }
        ICouponRepository Coupons { get; }
        IAuthRepository Auth { get; }
        IGlobalSettingRepository GlobalSettings { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
