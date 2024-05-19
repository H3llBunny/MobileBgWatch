using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public interface IScraperService
    {
        Task<IEnumerable<string>> GetAllVehicleAdUrlsAsync(string searchUrl);

        Task<ICollection<Vehicle>> CreateVehiclesListAsync(IEnumerable<string> vehicleUrls, string userId, string searchUrl);
    }
}
