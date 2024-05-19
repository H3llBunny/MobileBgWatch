using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public interface IVehicleService
    {
        Task AddVehicleAsync(ICollection<Vehicle> vehicles);

        Task<SearchUrlsListViewModel> GetSearchUrlsAsync(string userId);

        Task AddSearchUrlToUserAsync(string userId, string searchUrl);
    }
}
