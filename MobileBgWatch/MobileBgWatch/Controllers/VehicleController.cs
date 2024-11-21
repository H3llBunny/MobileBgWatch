using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Models;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;
using System.Security.Claims;

namespace MobileBgWatch.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            this._vehicleService = vehicleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetVehicleAd(long vehicleAdId)
        {
            if (!this._vehicleService.DoesVehicleIdExist(vehicleAdId))
            {
                TempData["ErrorMessage"] = "Vehicle Id does not exist.";
                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var vehicleViewModel = await this._vehicleService.GetVehicleByAdIdAsync(vehicleAdId);
            return this.View(vehicleViewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest requet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                await this._vehicleService.UpdateFavorite(requet.VehicleId, requet.Favorite);
                return Ok(new { success = true, favorite = requet.Favorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFavorites(int pageNumber = 1, string sortOrder = "newest")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction(nameof(UserController.Login), "User");
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
                VehiclesCount = await this._vehicleService.GetTotalFavoritesCountAsync(userId),
                Vehicles = await this._vehicleService.GetFavoties(userId, pageNumber, VehiclesPerPage, sortOrder),
                SortOrder = sortOrder
            };

            return this.View(viewModel);
        }
    }
}
