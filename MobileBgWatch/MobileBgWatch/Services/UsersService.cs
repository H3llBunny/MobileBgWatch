using Microsoft.AspNetCore.Identity;
using MobileBgWatch.Models;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<ApplicationUser> _userCollection;
        private readonly IVehicleService _vehicleService;

        public UsersService(UserManager<ApplicationUser> userManager, IMongoCollection<ApplicationUser> userCollection, IVehicleService vehicleService)
        {
            this._userManager = userManager;
            this._userCollection = userCollection;
            this._vehicleService = vehicleService;
        }

        public async Task<bool> UserSearchUrlLimitAsync(string userId)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            return user.SearchUrls.Count <= 5;
        }

        public async Task<bool> SearchUrlAlreadyExist(string userId, string searchUrl)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            return user.SearchUrls.Any(u => u.Url == searchUrl);
        }
        public async Task AddSearchUrlToUserAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var update = Builders<ApplicationUser>.Update.Push(u => u.SearchUrls, new SearchUrl { Url = searchUrl, LastRefresh = DateTime.UtcNow });

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task DeleteSearchUrlAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var update = Builders<ApplicationUser>.Update.PullFilter(u => u.SearchUrls,
                Builders<SearchUrl>.Filter.Eq(s => s.Url, searchUrl));

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
            await this._vehicleService.DeleteVehiclesForSearchUrlAsync(userId, searchUrl);
        }
        public async Task<bool> NewPasswordCheckAsync(string userId, string newPassword)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            var isNewPasswordTheSame = await this._userManager.CheckPasswordAsync(user, newPassword);
            if (!isNewPasswordTheSame)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task UpdatePasswordAsync(string userId, string newPassword)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var newPasswordHash = this._userManager.PasswordHasher.HashPassword(user, newPassword);

                var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
                var update = Builders<ApplicationUser>.Update.Set(u => u.PasswordHash, newPasswordHash);

                var options = new FindOneAndUpdateOptions<ApplicationUser>
                {
                    ReturnDocument = ReturnDocument.After
                };

                await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
            }
        }
    }
}
