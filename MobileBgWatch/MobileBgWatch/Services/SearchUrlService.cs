
using MobileBgWatch.Models;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class SearchUrlService : ISearchUrlService
    {
        private readonly IMongoCollection<ApplicationUser> _userCollection;

        public SearchUrlService(IMongoCollection<ApplicationUser> userCollection)
        {
            this._userCollection = userCollection;
        }
        public async Task<bool> CanRefreshAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId);
            var user = await this._userCollection.Find(filter).FirstOrDefaultAsync();
            var searchUrlEntity = user.SearchUrls.FirstOrDefault(s => s.Url == searchUrl);

            return searchUrlEntity.LastRefresh.AddMinutes(15) <= DateTime.UtcNow;
        }

        public async Task UpdateLastRefreshAsync(string userId, string searchUrl)
        {
            var filter = Builders<ApplicationUser>.Filter.And(
                Builders<ApplicationUser>.Filter.Eq(u => u.Id, userId),
                Builders<ApplicationUser>.Filter.ElemMatch(u => u.SearchUrls, su => su.Url == searchUrl));

            var update = Builders<ApplicationUser>.Update
                .Set("SearchUrls.$.LastRefresh", DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<ApplicationUser>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await this._userCollection.FindOneAndUpdateAsync(filter, update, options);
        }
    }
}
