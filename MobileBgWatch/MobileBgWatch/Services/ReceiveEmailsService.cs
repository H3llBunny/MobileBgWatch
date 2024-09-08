using MobileBgWatch.Models;
using MongoDB.Driver;

namespace MobileBgWatch.Services
{
    public class ReceiveEmailsService
    {
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoCollection<UserForEmailing> _usersForEmailing;

        public ReceiveEmailsService(IMongoCollection<ApplicationUser> usersCollection, IMongoCollection<UserForEmailing> usersForEmailing)
        {
            this._usersCollection = usersCollection;
            this._usersForEmailing = usersForEmailing;
        }

        public async Task RecieveEmails(string userId, bool receiveEmails)
        {
            var user = await this._usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (user.ReceiveEmails == receiveEmails)
            {
                return;
            }

            var update = Builders<ApplicationUser>.Update.Set(u => u.ReceiveEmails, receiveEmails);
            await this._usersCollection.UpdateOneAsync(u => u.Id == userId, update);

            if (receiveEmails)
            {
                var userForEmailing = new UserForEmailing
                {
                    UserId = user.Id,
                    UserEmail = user.Email
                };
                await this._usersForEmailing.InsertOneAsync(userForEmailing);
            }
            else
            {
                await this._usersForEmailing.FindOneAndDeleteAsync(u => u.UserId == userId);
            }
        }
    }
}
