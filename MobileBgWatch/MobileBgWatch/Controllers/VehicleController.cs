using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Services;

namespace MobileBgWatch.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            this.vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleAd(long vehicleAdId)
        {
            var vehicleViewModel = await this.vehicleService.GetVehicleByAdIdAsync(vehicleAdId);
            return this.View(vehicleViewModel);
        }
    }
}
