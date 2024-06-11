using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Controllers
{
    public class AllAdsController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public AllAdsController(IVehicleService vehicleService)
        {
            this._vehicleService = vehicleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAds(string searchUrl, int pageNumber = 1, string sortOrder = "newest")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("User/Login");
            }

            if (pageNumber <= 0)
            {
                return this.NotFound();
            }

            const int VehiclesPerPage = 36;

            var viewModel = new VehiclesListViewModel
            {
                VehiclesPerPage = VehiclesPerPage,
                PageNumber = pageNumber,
                SearchUrl = searchUrl,
                VehiclesCount = await this._vehicleService.GetTotalAdsCountAsync(searchUrl),
                Vehicles = await this._vehicleService.GetAllAsync(searchUrl, pageNumber, VehiclesPerPage, sortOrder),
                SortOrder = sortOrder
            };

            return this.View(viewModel);
        }
    }
}
