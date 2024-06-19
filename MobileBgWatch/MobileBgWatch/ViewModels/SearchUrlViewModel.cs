using MobileBgWatch.Models;

namespace MobileBgWatch.ViewModels
{
    public class SearchUrlViewModel
    {
        public string SearchUrl { get; set; }

        public int TotalAdsCount { get; set; }

        public DateTime LastRefresh { get; set; }

        public IEnumerable<VehicleInListViewModel> Vehicles { get; set; }
    }
}
