
using MobileBgWatch.Models;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class SearchUrlService : ISearchUrlService
    {
        private readonly IMongoCollection<ApplicationUser> _userCollection;
        private const int CooldownTime = 15;

        public SearchUrlService(IMongoCollection<ApplicationUser> userCollection)
        {
            this._userCollection = userCollection;
        }
        public async Task<bool> CanRefreshAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(filter).FirstOrDefaultAsync();
            var searchUrlEntity = user.SearchUrls.FirstOrDefault(s => s.Url == searchUrl);

            return searchUrlEntity.LastRefreshByUser.AddMinutes(CooldownTime) <= DateTime.UtcNow;
        }

        public async Task UpdateLastRefreshAsync(string userId, string searchUrl)
        {
            var filter = GetUserAndSearchUrlFilter(userId, searchUrl);

            var update = Builders<ApplicationUser>.Update
                .Set("SearchUrls.$.LastRefreshByUser", DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task UpdateLastRefreshByServiceAsync(string userId, string searchUrl)
        {
            var filter = GetUserAndSearchUrlFilter(userId, searchUrl);

            var update = Builders<ApplicationUser>.Update
                .Set("SearchUrls.$.LastRefreshByService", DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task ResetRefreshCounterAsync(string userId, string searchUrl)
        {
            var filter = GetUserAndSearchUrlFilter(userId, searchUrl);

            var update = Builders<ApplicationUser>.Update.Set("SearchUrls.$.RefreshCounter", 0);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task UpdateRefreshCounterAsync(string userId, string searchUrl)
        {
            var filter = GetUserAndSearchUrlFilter(userId, searchUrl);

            var update = Builders<ApplicationUser>.Update.Inc("SearchUrls.$.RefreshCounter", 1);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<bool> DoesUrlExistAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(filter).FirstOrDefaultAsync();
            return user.SearchUrls.Any(s => s.Url == searchUrl);
        }

        private FilterDefinition<ApplicationUser> GetUserAndSearchUrlFilter(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.And(
                Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId),
                Builders<ApplicationUser>.Filter.ElemMatch(u => u.SearchUrls, su => su.Url == searchUrl));

            return filter;
        }
    }
}
