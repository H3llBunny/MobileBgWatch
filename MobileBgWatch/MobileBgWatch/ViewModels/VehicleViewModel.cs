using MobileBgWatch.Models;

namespace MobileBgWatch.ViewModels
{
    public class VehicleViewModel
    {
        public string Id { get; set; }
        public long VehicleAdId { get; set; }

        public string SearchUrl { get; set; }

        public string Name { get; set; }

        public DateTime DateAdded { get; set; }

        public VehiclePrice CurrentPrice { get; set; }
        
        public VehiclePrice PreviousPrice { get; set; }

        public string Url { get; set; }

        public string Location { get; set; }

        public Dictionary<string, string> Specifications { get; set; }

        public List<string> ImageUrls { get; set; }

        public List<VehiclePrice> HistoricalPrices { get; set; }

        public bool Favorite { get; set; }
    }
}
