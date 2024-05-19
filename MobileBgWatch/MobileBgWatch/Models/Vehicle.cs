using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace MobileBgWatch.Models
{
    [CollectionName("Vehicles")]
    public class Vehicle
    {
        public Vehicle()
        {
            this.HistoricalPrices = new List<VehiclePrice>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string SearchUrl { get; set; }

        public long VehicleAdId { get; set; }

        public string Name { get; set; }

        public DateTime DateAdded { get; set; }

        public VehiclePrice CurrentPrice { get; set; }

        public VehiclePrice PreviousPrice { get; set; }

        public string Url { get; set; }

        public string Location { get; set; }

        public Dictionary<string, string> Specifications { get; set; }

        public List<string> ImageUrls { get; set; }

        public List<VehiclePrice> HistoricalPrices { get; set; }
    }
}
