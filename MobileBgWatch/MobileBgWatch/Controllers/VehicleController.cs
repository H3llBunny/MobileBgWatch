using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;

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
        public async Task<IActionResult> GetVehicleAd(long vehicleAdId)
        {
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
    }
}
