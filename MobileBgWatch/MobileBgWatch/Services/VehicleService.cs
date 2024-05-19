using Microsoft.AspNetCore.Identity;
using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IMongoCollection<Vehicle> _vehiclesCollection;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleService(MongoDbConfig config, UserManager<ApplicationUser> userManager)
        {
            var client = new MongoClient(config.ConnectionString);
            var database = client.GetDatabase(config.Database);
            this._vehiclesCollection = database.GetCollection<Vehicle>("Vehicles");
            this._userManager = userManager;
        }

        public async Task AddVehicleAsync(ICollection<Vehicle> vehicles)
        {
            await this._vehiclesCollection.InsertManyAsync(vehicles);
        }

        public async Task<SearchUrlsListViewModel> GetSearchUrlsAsync(string userId)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            if (user.SearchUrls == null || user.SearchUrls?.Count == 0)
            {
                return null;
            }

            var searchUrlList = new SearchUrlsListViewModel();

            foreach (var url in user.SearchUrls)
            {
                var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, url);
                var vehicles = await this._vehiclesCollection.Find(filter).ToListAsync();
                var searchUrlModel = new SearchUrlViewModel
                {
                    SearchUrl = url,
                    Vehicles = vehicles.Select(v => new VehicleInListViewModel
                    {
                        Id = v.Id,
                        ImageUrl = v.ImageUrls.FirstOrDefault(),
                        Name = v.Name,
                        CurrentPrice = v.CurrentPrice,
                        PreviousPrice = v.PreviousPrice
                    }).ToList()
                };

                searchUrlList.SearchUrls.Add(searchUrlModel);
            }

            return searchUrlList;
        }

        public async Task AddSearchUrlToUserAsync(string userId, string searchUrl)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            user.SearchUrls.Add(searchUrl);
        }
    }
}
