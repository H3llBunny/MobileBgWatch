using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IMongoCollection<Vehicle> _vehiclesCollection;
        private readonly IMongoCollection<ApplicationUser> _userCollection;

        public VehicleService(MongoDbConfig config, IMongoCollection<ApplicationUser> userCollection)
        {
            var client = new MongoClient(config.ConnectionString);
            var database = client.GetDatabase(config.Database);
            this._vehiclesCollection = database.GetCollection<Vehicle>("Vehicles");
            this._userCollection = userCollection;
        }

        public async Task AddVehicleAsync(ICollection<Vehicle> vehicles)
        {
            var removeIdentifiers = new List<long>();

            foreach (var vehicle in vehicles)
            {
                if (DoesVehicleAdExist(vehicle))
                {
                    if (await ChangeInPriceAsync(vehicle))
                    {
                        await UpdateVehiclePrice(vehicle);
                        removeIdentifiers.Add(vehicle.VehicleAdId);
                        continue;
                    }

                    removeIdentifiers.Add(vehicle.VehicleAdId);
                }
            }

            var vehicleList = vehicles.Reverse().ToList();
            vehicleList.RemoveAll(v => removeIdentifiers.Contains(v.VehicleAdId));
            if (vehicleList.Count > 0)
            {
                await this._vehiclesCollection.InsertManyAsync(vehicleList);
            }
        }

        public async Task<SearchUrlsListViewModel> GetSearchUrlsListAsync(string userId, int? count = null)
        {
            var userFilter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(userFilter).FirstOrDefaultAsync();

            if (user.SearchUrls == null || user.SearchUrls?.Count == 0)
            {
                return null;
            }

            var searchUrlList = new SearchUrlsListViewModel();

            foreach (var url in user.SearchUrls)
            {
                var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, url);
                var vehiclesQuery = this._vehiclesCollection.Find(filter).SortByDescending(v => v.Id);
                var vehicles = await (count.HasValue ? vehiclesQuery.Limit(count.Value) : vehiclesQuery).ToListAsync();
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
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(filter).FirstOrDefaultAsync();
            var update = Builders<ApplicationUser>.Update
                .Push(u => u.SearchUrls, searchUrl);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public bool DoesVehicleAdExist(Vehicle vehicle)
        {
            return this._vehiclesCollection.AsQueryable().Any(v => v.VehicleAdId == vehicle.VehicleAdId);
        }

        public async Task<bool> ChangeInPriceAsync(Vehicle vehicle)
        {
            var vehicleFromDb = await this._vehiclesCollection.Find(v => v.VehicleAdId == vehicle.VehicleAdId).FirstOrDefaultAsync();
            return vehicleFromDb.CurrentPrice.Price != vehicle.CurrentPrice.Price;
        }

        public async Task UpdateVehiclePrice(Vehicle vehicle)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.VehicleAdId, vehicle.VehicleAdId);

            var vehicleFromDb = await this._vehiclesCollection.Find(filter).FirstOrDefaultAsync();

            var update = Builders<Vehicle>.Update
                .Set(v => v.PreviousPrice, vehicleFromDb.CurrentPrice)
                .Set(v => v.CurrentPrice, vehicle.CurrentPrice)
                .Push(v => v.HistoricalPrices, vehicleFromDb.CurrentPrice);

            var options = new FindOneAndUpdateOptions<Vehicle>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._vehiclesCollection.FindOneAndUpdateAsync(filter, update, options);
        }
    }
}
