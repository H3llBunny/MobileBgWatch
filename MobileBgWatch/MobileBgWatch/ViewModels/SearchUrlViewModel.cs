namespace MobileBgWatch.ViewModels
{
    public class SearchUrlViewModel
    {
        public string SearchUrl { get; set; }

        public IEnumerable<VehicleInListViewModel> Vehicles { get; set; }
    }
}
