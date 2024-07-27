namespace MobileBgWatch.ViewModels
{
    public class FavoriteVehicles : PagingViewModel
    {
        public string SortOrder { get; set; }

        public IEnumerable<VehicleInListViewModel> Vehicles { get; set; }
    }
}
