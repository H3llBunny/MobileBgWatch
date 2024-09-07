using MobileBgWatch.Models;
using MongoDB.Driver;
using SendGrid.Helpers.Mail.Model;

namespace MobileBgWatch.Services
{
    public class AutoRefreshAdsService : BackgroundService
    {
        private readonly IMongoCollection<ApplicationUser> _userCollection;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AutoRefreshAdsService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public AutoRefreshAdsService(
            IMongoCollection<ApplicationUser> userCollection,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AutoRefreshAdsService> logger,
            INotificationService notificationService,
            IEmailService emailService)

        {
            this._userCollection = userCollection;
            this._serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
            this._notificationService = notificationService;
            this._emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = this._serviceScopeFactory.CreateScope())
                    {
                        var vehicleService = scope.ServiceProvider.GetRequiredService<IVehicleService>();
                        var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
                        var searchUrlService = scope.ServiceProvider.GetRequiredService<ISearchUrlService>();

                        await DoWorkAsync(vehicleService, scraperService, searchUrlService);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured while executing the background task.");
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task DoWorkAsync(
            IVehicleService vehicleService,
            IScraperService scraperService,
            ISearchUrlService searchUrlService)
        {
            var users = await this._userCollection.Find(_ => true).ToListAsync();

            foreach (var user in users)
            {
                var newVehicleAds = new List<List<Vehicle>>();

                foreach (var url in user?.SearchUrls)
                {
                    if (url.RefreshCounter == 3 && (DateTime.UtcNow - url.LastRefreshByService).TotalMinutes >= 15)
                    {
                        var allUrls = (await scraperService.GetAllVehicleAdUrlsAsync(url.Url)).ToList();
                        var vehicleList = await scraperService.CreateVehiclesListAsync(allUrls, user.Id, url.Url);
                        var addedVehicles = (await vehicleService.AddVehicleAsync(vehicleList)).ToList();

                        if(addedVehicles.Count > 0)
                        {
                            newVehicleAds.Add(addedVehicles);
                        }

                        await vehicleService.DeletedSoldVehiclesAsync(vehicleList);

                        await searchUrlService.UpdateLastRefreshByServiceAsync(user.Id, url.Url);
                        await searchUrlService.ResetRefreshCounterAsync(user.Id, url.Url);
                    }
                    else if (url.RefreshCounter < 3 && (DateTime.UtcNow - url.LastRefreshByService).TotalMinutes >= 15)
                    {
                        var newUrls = (await scraperService.GetAllVehicleAdUrlsAsync(url.Url, user.Id, true)).ToList();

                        if (newUrls.Count > 0)
                        {
                            var newVehicles = await scraperService.CreateVehiclesListAsync(newUrls, user.Id, url.Url);
                            var addedVehicles = (await vehicleService.AddVehicleAsync(newVehicles)).ToList();

                            if (addedVehicles.Count > 0)
                            {
                                newVehicleAds.Add(addedVehicles);
                            }
                        }

                        await searchUrlService.UpdateLastRefreshByServiceAsync(user.Id, url.Url);
                        await searchUrlService.UpdateRefreshCounterAsync(user.Id, url.Url);
                    }
                }

                if (newVehicleAds.Count > 0)
                {
                    var newAdsCount = newVehicleAds.Sum(v => v.Count);
                    var notificationMessage = $"You have {newAdsCount} new ads";
                    var subject = $"You have {newAdsCount} new ads!";

                    await this._notificationService.SendNotificationAsync(user.Id, notificationMessage);

                    await _emailService.SendEmailAsync(user.Email, subject, newVehicleAds);
                }
            }
        }
    }
}
