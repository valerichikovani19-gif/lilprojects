// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;

namespace Discounts.Application.RepoInterfaces
{
    public interface IOfferRepository : IGenericRepository<Offer>
    {
        Task<List<Offer>> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken);
        Task<List<Offer>> GetAllActiveAsync(CancellationToken cancellationToken);
        //
        Task<List<Offer>> GetPendingOffersAsync(CancellationToken cancellationToken);

        Task UpdateExpirationStatusAsync(CancellationToken cancellationToken);
        Task<List<Offer>> GetAllActiveAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken);
        Task<(List<Offer> Offers, int TotalCount)> GetAllActiveAsync( string? search,int? categoryId,decimal? minPrice,decimal? maxPrice,int pageIndex,int pageSize,CancellationToken cancellationToken);
    }
}
