using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public interface IVehicleService
    {
        Task<ICollection<Vehicle>> AddVehicleAsync(ICollection<Vehicle> vehicles);

        Task<SearchUrlsListViewModel> GetSearchUrlsListAsync(string userId, int? count = null);

        Task<IEnumerable<VehicleInListViewModel>> GetAllAsync(string userId, string searchUrl, int pageNumber, int vehiclesPerPage, string sortOrder);

        bool DoesVehicleAdExist(Vehicle vehicle);

        Task<bool> ChangeInPriceAsync(Vehicle vehicle);

        Task UpdateVehiclePriceAsync(Vehicle vehicle);

        Task<int> GetTotalAdsCountAsync(string userId, string searchUrl);

        Task DeletedSoldVehiclesAsync(ICollection<Vehicle> currentVehicles);

        Task ImageUrlUpdaterAsync(Vehicle vehicle);

        Task<VehicleViewModel> GetVehicleByAdIdAsync(long vehicleAdId);

        Task DeleteVehiclesForSearchUrlAsync(string userId, string searchUrl);

        Task UpdateFavorite(string vehicleId, bool favorite);

        Task<int> GetTotalFavoritesCountAsync(string userId);

        Task<IEnumerable<VehicleInListViewModel>> GetFavoties(string userId, int pageNumber, int VehiclesPerPage, string sortOrder);

        Task<bool> CheckAdExistAsync(string userId, string url);
    }
}
