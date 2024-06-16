using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MobileBgWatch.Models;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<ApplicationUser> _userCollection;

        public UsersService(UserManager<ApplicationUser> userManager, IMongoCollection<ApplicationUser> userCollection)
        {
            this._userManager = userManager;
            this._userCollection = userCollection;
        }

        public async Task<bool> UserSearchUrlLimitAsync(string userId)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            return user.SearchUrls.Count <= 5;
        }

        public async Task<bool> SearchUrlAlreadyExist(string userId, string searchUrl)
        {
            var user = await this._userManager.FindByIdAsync(userId);
            return user.SearchUrls.Contains(searchUrl);
        }
        public async Task AddSearchUrlToUserAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var update = Builders<ApplicationUser>.Update
                .Push(u => u.SearchUrls, searchUrl);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task DeleteSearchUrlAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var update = Builders<ApplicationUser>.Update
                .Pull(u => u.SearchUrls, searchUrl);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
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
