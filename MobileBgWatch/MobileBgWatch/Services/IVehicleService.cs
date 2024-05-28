using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public interface IVehicleService
    {
        Task AddVehicleAsync(ICollection<Vehicle> vehicles);

        Task<SearchUrlsListViewModel> GetSearchUrlsListAsync(string userId, int? count = null);

        Task<IEnumerable<VehicleInListViewModel>> GetAllAsync(string searchUrl, int page, int vehiclesPerPage);

        bool DoesVehicleAdExist(Vehicle vehicle);

        Task<bool> ChangeInPriceAsync(Vehicle vehicle);

        Task UpdateVehiclePriceAsync(Vehicle vehicle);

        Task<int> GetTotalAdsCountAsync(string searchUrl);

        Task DeletedSoldVehiclesAsync(ICollection<Vehicle> currentVehicles);

        Task ImageUrlUpdaterAsync(Vehicle vehicle);

        Task<VehicleViewModel> GetVehicleByAdIdAsync(long vehicleAdId);
    }
}
