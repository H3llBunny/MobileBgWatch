namespace MobileBgWatch.ViewModels
{
    public class VehiclesListViewModel : PagingViewModel
    {
        public string SearchUrl { get; set; }

        public string SortOrder { get; set; }

        public IEnumerable<VehicleInListViewModel> Vehicles { get; set; }
    }
}
