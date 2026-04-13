// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.BackgroundServices
{
    public class ReservationCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservationCleanupWorker> _logger;

        public ReservationCleanupWorker(IServiceProvider serviceProvider, ILogger<ReservationCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("reservation cleanup worker started");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var settings = await unitOfWork.GlobalSettings.GetAsync(ct).ConfigureAwait(false);
                        var timeoutMinutes = settings.ReservationTimeInMinutes;
                        var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
                        var expiredCoupons = await unitOfWork.Coupons.FindExpiredReservationsAsync(cutoffTime, ct).ConfigureAwait(false);

                        if (expiredCoupons.Any())
                        {
                            foreach (var coupon in expiredCoupons)
                            {
                                var offer = await unitOfWork.Offers.GetByIdAsync(coupon.OfferId, ct).ConfigureAwait(false);
                                if (offer != null)
                                {
                                    offer.AvailableQuantity += 1;
                                    unitOfWork.Offers.Update(offer);
                                }
                                unitOfWork.Coupons.Remove(coupon);
                            }

                            await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                            _logger.LogInformation($"Cleaned up {expiredCoupons.Count} expired reservations");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "error cleaning reservations");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), ct).ConfigureAwait(false);
            }
        }
    }
}
