// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Common;
using Discounts.Application.DTOs.Merchant;
using Discounts.Application.DTOs.Offer;

namespace Discounts.Application.ServiceInterfaces
{
    public interface IOfferService
    {
        //merchant
        Task<OfferDto> GetOfferByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> CreateOfferAsync(CreateOfferDto offerDto, string merchantId, CancellationToken cancellationToken);
        Task UpdateOfferAsync(int id, UpdateOfferDto offerDto, string merchantId, CancellationToken cancellationToken);
        Task<List<OfferDto>> GetMerchantOffersAsync(string merchantId, CancellationToken cancellationToken);
        Task<List<MerchantSalesDto>> GetMerchantSalesHistoryAsync(string merchantId, CancellationToken cancellationToken);
        Task<MerchantDashboardDto> GetMerchantDashboardAsync(string merchantId, CancellationToken cancellationToken);
        Task DeleteOfferAsync(int id, string merchantId, CancellationToken cancellationToken);        

        //admin
        Task<List<OfferDto>> GetPendingOffersAsync(CancellationToken cancellationToken);
        Task ApproveOfferAsync(int id, CancellationToken cancellationToken);
        Task RejectOfferAsync(int id, RejectOfferDto rejectDto, CancellationToken cancellationToken);
        Task DeleteOfferByAdminAsync(int id, CancellationToken cancellationToken);

        //customer
        Task<List<OfferDto>> GetActiveOffersAsync(CancellationToken cancellationToken);

        Task<List<OfferDto>> GetActiveOffersAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken);
        Task<PagedResponse<OfferDto>> GetActiveOffersAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int pageIndex, int pageSize, CancellationToken cancellationToken);

    }
}
