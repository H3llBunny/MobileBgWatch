using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace MobileBgWatch.Models
{
    [CollectionName("UsersForEmailing")]
    public class UserForEmailing
    {
        public UserForEmailing()
        {
            this.NewVehicles = new List<Vehicle>();
        }

        [BsonId]
        public Object Id { get; set; }

        public string UserId { get; set; }

        public string UserEmail { get; set; }

        public List<Vehicle> NewVehicles { get; set; }
    }
}
