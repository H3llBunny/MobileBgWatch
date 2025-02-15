﻿using AutoMapper;
using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IMongoCollection<Vehicle> _vehiclesCollection;
        private readonly IMongoCollection<ApplicationUser> _userCollection;
        private readonly IMapper _mapper;

        public VehicleService(MongoDbConfig config, IMongoCollection<ApplicationUser> userCollection, IMapper mapper)
        {
            var client = new MongoClient(config.ConnectionString);
            var database = client.GetDatabase(config.Database);
            this._vehiclesCollection = database.GetCollection<Vehicle>("Vehicles");
            this._userCollection = userCollection;
            this._mapper = mapper;
        }

        public async Task<ICollection<Vehicle>> AddVehicleAsync(ICollection<Vehicle> vehicles)
        {
            var removeIdentifiers = new List<long>();

            foreach (var vehicle in vehicles)
            {
                if (DoesVehicleAdExist(vehicle))
                {
                    await ImageUrlUpdaterAsync(vehicle);
                    
                    if (await ChangeInPriceAsync(vehicle))
                    {
                        await UpdateVehiclePriceAsync(vehicle);
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

                return vehicleList;
            }

            return vehicleList;
        }

        public async Task<SearchUrlsListViewModel> GetSearchUrlsListAsync(string userId, int? count = null)
        {
            var userFilter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(userFilter).FirstOrDefaultAsync();

            if (user?.SearchUrls == null || user.SearchUrls?.Count == 0)
            {
                return null;
            }

            var searchUrlList = new SearchUrlsListViewModel();

            foreach (var url in user.SearchUrls)
            {
                var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, url.Url) & Builders<Vehicle>.Filter.Eq(v => v.UserId, userId);
                var vehiclesQuery = this._vehiclesCollection.Find(filter).SortByDescending(v => v.Id);
                int totalAdsCount = await GetTotalAdsCountAsync(userId, url.Url);
                var vehicles = await (count.HasValue ? vehiclesQuery.Limit(count.Value) : vehiclesQuery).ToListAsync();
                var searchUrlModel = new SearchUrlViewModel
                {
                    SearchUrl = url.Url,
                    TotalAdsCount = totalAdsCount,
                    LastRefresh = url.LastRefreshByUser,
                    Vehicles = vehicles.Select(v => new VehicleInListViewModel
                    {
                        Id = v.Id,
                        ImageUrl = v.ImageUrls.FirstOrDefault(),
                        Name = v.Name,
                        DateAdded = v.DateAdded,
                        VehicleAdId = v.VehicleAdId,
                        CurrentPrice = v.CurrentPrice,
                        PreviousPrice = v.PreviousPrice,
                        Favorite = v.Favorite
                    }).ToList()
                };

                searchUrlList.SearchUrls.Add(searchUrlModel);
            }

            searchUrlList.SearchUrls.Reverse();
            var reversedSearchUrlList = new SearchUrlsListViewModel
            {
                SearchUrls = searchUrlList.SearchUrls.Reverse().ToList()
            };

            return reversedSearchUrlList;
        }

        public bool DoesVehicleAdExist(Vehicle vehicle)
        {
            return this._vehiclesCollection.AsQueryable()
                .Any(v => v.VehicleAdId == vehicle.VehicleAdId
                        && v.SearchUrl == vehicle.SearchUrl
                        && v.UserId == vehicle.UserId);
        }

        public bool DoesVehicleIdExist(long vehicleId)
        {
            return this._vehiclesCollection.AsQueryable()
                .Any(v => v.VehicleAdId == vehicleId);
        }

        public async Task<bool> ChangeInPriceAsync(Vehicle vehicle)
        {
            var vehicleFromDb = await this._vehiclesCollection.Find(v => v.VehicleAdId == vehicle.VehicleAdId).FirstOrDefaultAsync();
            return vehicleFromDb.CurrentPrice.Price != vehicle.CurrentPrice.Price;
        }

        public async Task UpdateVehiclePriceAsync(Vehicle vehicle)
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

        public async Task<int> GetTotalAdsCountAsync(string userId, string searchUrl)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, searchUrl) & Builders<Vehicle>.Filter.Eq(v => v.UserId, userId);
            var vehiclesQuery = this._vehiclesCollection.Find(filter).SortByDescending(v => v.Id);
            var totalVehicles = await vehiclesQuery.ToListAsync();
            return totalVehicles.Count();
        }

        public async Task<IEnumerable<VehicleInListViewModel>> GetAllAsync(string userId, string searchUrl, int pageNumber, int vehiclesPerPage, string sortOder)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, searchUrl) & Builders<Vehicle>.Filter.Eq(v => v.UserId, userId);
            var projection = Builders<Vehicle>.Projection.Expression(v => new VehicleInListViewModel
            {
                Id = v.Id,
                ImageUrl = v.ImageUrls.FirstOrDefault(),
                Name = v.Name,
                DateAdded = v.DateAdded,
                VehicleAdId = v.VehicleAdId,
                CurrentPrice = v.CurrentPrice,
                PreviousPrice = v.PreviousPrice,
                Favorite = v.Favorite
            });

            var query = this._vehiclesCollection.Find(filter);
              
            switch (sortOder)
            {
                case "price_asc":
                    query = query.SortBy(v => v.CurrentPrice.Price);
                    break;
                case "price_desc":
                    query = query.SortByDescending(v => v.CurrentPrice.Price);
                    break;
                default:
                    query = query.SortByDescending(v => v.Id);
                    break;
            }

            var vehicles = await query
                .Skip((pageNumber - 1) * vehiclesPerPage)
                .Limit(vehiclesPerPage)
                .Project(projection)
                .ToListAsync();

            return vehicles;
        }

        public async Task DeletedSoldVehiclesAsync(ICollection<Vehicle> currentVehicles)
        {
            try
            {
                var currentVehicleAdIds = currentVehicles.Select(cv => cv.VehicleAdId).ToList();

                var filter = Builders<Vehicle>.Filter.And(
                    Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, currentVehicles.FirstOrDefault()?.SearchUrl),
                    Builders<Vehicle>.Filter.Eq(v => v.UserId, currentVehicles.FirstOrDefault()?.UserId),
                    Builders<Vehicle>.Filter.Not(Builders<Vehicle>.Filter.In(v => v.VehicleAdId, currentVehicleAdIds)));

                await this._vehiclesCollection.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        public async Task ImageUrlUpdaterAsync(Vehicle vehicle)
        {
            var vehicleFromDb = await this._vehiclesCollection.Find(v => v.VehicleAdId == vehicle.VehicleAdId).FirstOrDefaultAsync();
            if (!vehicleFromDb.ImageUrls.SequenceEqual(vehicle.ImageUrls))
            {
                vehicleFromDb.ImageUrls = vehicle.ImageUrls;
                await this._vehiclesCollection.ReplaceOneAsync(v => v.VehicleAdId == vehicleFromDb.VehicleAdId, vehicleFromDb);
            }
        }

        public async Task<VehicleViewModel> GetVehicleByAdIdAsync(long vehicleAdId)
        {
            var vehicleFromDb = await this._vehiclesCollection.Find(v => v.VehicleAdId == vehicleAdId).FirstOrDefaultAsync();

            return this._mapper.Map<VehicleViewModel>(vehicleFromDb);
        }

        public async Task DeleteVehiclesForSearchUrlAsync(string userId, string searchUrl)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.SearchUrl, searchUrl) & Builders<Vehicle>.Filter.Eq(v => v.UserId, userId);
            await this._vehiclesCollection.DeleteManyAsync(filter);
        }

        public async Task UpdateFavorite(string vehicleId, bool favorite)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.Id, vehicleId);
            var update = Builders<Vehicle>.Update.Set(v => v.Favorite, favorite);

            var result = await this._vehiclesCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("No vehicle was found with the provided ID.");
            }
        }
        public async Task<int> GetTotalFavoritesCountAsync(string userId)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.UserId, userId) & Builders<Vehicle>.Filter.Eq(v => v.Favorite, true);
            var vehiclesQuery = this._vehiclesCollection.Find(filter).SortByDescending(v => v.Id);
            var totalVehicles = await vehiclesQuery.ToListAsync();
            return totalVehicles.Count();
        }

        public async Task<IEnumerable<VehicleInListViewModel>> GetFavoties(string userId, int pageNumber, int vehiclesPerPage, string sortOder)
        {
            var filter = Builders<Vehicle>.Filter.Eq(v => v.UserId, userId) & Builders<Vehicle>.Filter.Eq(v => v.Favorite, true);
            var projection = Builders<Vehicle>.Projection.Expression(v => new VehicleInListViewModel
            {
                Id = v.Id,
                ImageUrl = v.ImageUrls.FirstOrDefault(),
                Name = v.Name,
                DateAdded = v.DateAdded,
                VehicleAdId = v.VehicleAdId,
                CurrentPrice = v.CurrentPrice,
                PreviousPrice = v.PreviousPrice,
                Favorite = v.Favorite
            });

            var query = this._vehiclesCollection.Find(filter);

            switch (sortOder)
            {
                case "price_asc":
                    query = query.SortBy(v => v.CurrentPrice.Price);
                    break;
                case "price_desc":
                    query = query.SortByDescending(v => v.CurrentPrice.Price);
                    break;
                default:
                    query = query.SortByDescending(v => v.Id);
                    break;
            }

            var vehicles = await query
                .Skip((pageNumber - 1) * vehiclesPerPage)
                .Limit(vehiclesPerPage)
                .Project(projection)
                .ToListAsync();

            return vehicles;
        }

        public async Task<bool> CheckAdExistAsync(string userId, string url)
        {
            var filter = Builders<Vehicle>.Filter.And(
                Builders<Vehicle>.Filter.Eq(v => v.UserId, userId),
                Builders<Vehicle>.Filter.Eq(v => v.Url, url));

            var result = await this._vehiclesCollection.Find(filter).FirstOrDefaultAsync();

            return result != null;
        }
    }
}
