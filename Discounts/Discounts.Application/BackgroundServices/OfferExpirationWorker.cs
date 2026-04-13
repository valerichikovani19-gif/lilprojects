// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.BackgroundServices
{
    public class OfferExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OfferExpirationWorker> _logger;

        public OfferExpirationWorker(IServiceProvider serviceProvider, ILogger<OfferExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Offer expiration worker started running");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        await unitOfWork.Offers.UpdateExpirationStatusAsync(stoppingToken).ConfigureAwait(false);
                        var changes = await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                        if (changes > 0)
                        {
                            _logger.LogInformation($"[Worker] Expired {changes} offers at {DateTime.UtcNow}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Worker] error checking for expired offers");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
