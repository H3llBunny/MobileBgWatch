using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;
using System.Security.Claims;

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

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var viewModel = new VehiclesListViewModel
            {
                VehiclesPerPage = VehiclesPerPage,
                PageNumber = pageNumber,
                SearchUrl = searchUrl,
                VehiclesCount = await this._vehicleService.GetTotalAdsCountAsync(userId, searchUrl),
                Vehicles = await this._vehicleService.GetAllAsync(userId, searchUrl, pageNumber, VehiclesPerPage, sortOrder),
                SortOrder = sortOrder
            };

            return this.View(viewModel);
        }
    }
}
