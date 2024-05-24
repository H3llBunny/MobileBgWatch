using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public interface IVehicleService
    {
        Task AddVehicleAsync(ICollection<Vehicle> vehicles);

        Task<SearchUrlsListViewModel> GetSearchUrlsListAsync(string userId, int? count = null);

        Task<IEnumerable<VehicleInListViewModel>> GetAllAsync(string searchUrl, int page, int vehiclesPerPage);

        Task AddSearchUrlToUserAsync(string userId, string searchUrl);

        bool DoesVehicleAdExist(Vehicle vehicle);

        Task<bool> ChangeInPriceAsync(Vehicle vehicle);

        Task UpdateVehiclePriceAsync(Vehicle vehicle);

        Task<int> GetTotalAdsCountAsync(string searchUrl);
    }
}
