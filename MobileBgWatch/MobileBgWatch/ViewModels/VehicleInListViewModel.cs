using MobileBgWatch.Models;

namespace MobileBgWatch.ViewModels
{
    public class VehicleInListViewModel
    {
        public string Id { get; set; }

        public string ImageUrl { get; set; }

        public string Name { get; set; }

        public DateTime DateAdded { get; set; }

        public long VehicleAdId { get; set; }

        public VehiclePrice CurrentPrice { get; set; }

        public VehiclePrice PreviousPrice { get; set; }
    }
}
