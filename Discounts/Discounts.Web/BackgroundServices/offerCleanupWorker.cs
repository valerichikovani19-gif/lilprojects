// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;

namespace Discounts.Web.BackgroundServices
{
    public class OfferCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OfferCleanupWorker> _logger;

        public OfferCleanupWorker(IServiceProvider serviceProvider, ILogger<OfferCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Worker Started - watching for expired offers and reservations...");
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await DoWorkAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker crashed during cleanup ");
                }
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var settings = await unitOfWork.GlobalSettings.GetAsync(stoppingToken).ConfigureAwait(false);
                var limitInMinutes = settings.ReservationTimeInMinutes;
                var cutoffTime = DateTime.UtcNow.AddMinutes(-limitInMinutes);

                var expiredCoupons = await unitOfWork.Coupons.FindExpiredReservationsAsync(cutoffTime, stoppingToken).ConfigureAwait(false);

                if (expiredCoupons.Any())
                {
                    foreach (var coupon in expiredCoupons)
                    {
                        var offer = await unitOfWork.Offers.GetByIdAsync(coupon.OfferId, stoppingToken).ConfigureAwait(false);
                        if (offer != null)
                        {
                            offer.AvailableQuantity += 1;
                            unitOfWork.Offers.Update(offer);
                        }

                        unitOfWork.Coupons.Delete(coupon);
                    }
                    _logger.LogInformation($"Worker: Released {expiredCoupons.Count} expired reservations.");
                }
                //expired oferebistvis
                await unitOfWork.Offers.UpdateExpirationStatusAsync(stoppingToken).ConfigureAwait(false);

                await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
