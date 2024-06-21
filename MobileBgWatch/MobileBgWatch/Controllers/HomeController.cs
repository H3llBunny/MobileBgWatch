using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Models;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace MobileBgWatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUsersService _usersService;
        private readonly IScraperService _scraperService;
        private readonly IVehicleService _vehicleService;
        private readonly ISearchUrlService _searchUrlService;

        public HomeController(ILogger<HomeController> logger, IUsersService usersService, IScraperService scraperService, IVehicleService vehicleService, ISearchUrlService searchUrlService)
        {
            _logger = logger;
            this._usersService = usersService;
            this._scraperService = scraperService;
            this._vehicleService = vehicleService;
            this._searchUrlService = searchUrlService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return this.View();
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            SearchUrlsListViewModel searchUrlsViewModel = await this._vehicleService.GetSearchUrlsListAsync(userId, 40);

            return this.View(searchUrlsViewModel);
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSearchUrl(string searchUrl)
        {
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                this.TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return this.RedirectToAction(nameof(this.Index));
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await this._usersService.UserSearchUrlLimitAsync(userId))
            {
                this.TempData["ErrorMessage"] = "You have reached the limit of 5 search URLs. I might add a monetization to expand it later in the development.";
                return this.RedirectToAction(nameof(this.Index));
            }

            if (await this._usersService.SearchUrlAlreadyExist(userId, searchUrl))
            {
                this.TempData["ErrorMessage"] = "Search Url already exist.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                var vehicleUrls = (await this._scraperService.GetAllVehicleAdUrlsAsync(searchUrl)).ToList();
                var vehicleList = await this._scraperService.CreateVehiclesListAsync(vehicleUrls, userId, searchUrl);
                await this._vehicleService.AddVehicleAsync(vehicleList);
                await this._usersService.AddSearchUrlToUserAsync(userId, searchUrl);
                this.TempData["SuccessMessage"] = "New search URL has been successfully added.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RefreshAds(string searchUrl)
        {
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                this.TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return this.RedirectToAction(nameof(this.Index));
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await this._usersService.UserSearchUrlLimitAsync(userId))
            {
                this.TempData["ErrorMessage"] = "You have reached the limit of 5 search URLs. I might add a monetization to expand it later in the development.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                if (!await this._searchUrlService.CanRefreshAsync(userId, searchUrl))
                {
                    this.TempData["ErrorMessage"] = "You can only refresh each URL once every 15 minutes.";
                    return this.RedirectToAction(nameof(this.Index));
                }

                var vehicleUrls = (await this._scraperService.GetAllVehicleAdUrlsAsync(searchUrl)).ToList();
                var vehicleList = await this._scraperService.CreateVehiclesListAsync(vehicleUrls, userId, searchUrl);
                await this._vehicleService.AddVehicleAsync(vehicleList);
                await this._vehicleService.DeletedSoldVehiclesAsync(vehicleList);

                await this._searchUrlService.UpdateLastRefreshAsync(userId, searchUrl);
            }
            catch (Exception ex)
            {
                this.TempData["ErrorMessage"] = ex.Message;
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DeleteUrl(string searchUrl)
        {
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                this.TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return this.RedirectToAction(nameof(this.Index));
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await this._usersService.DeleteSearchUrlAsync(userId, searchUrl);
                this.TempData["SuccessMessage"] = "Link has been successfully deleted.";
            }
            catch (Exception ex)
            {
                this.TempData["ErrorMessage"] = "An error occurred while updating the database.";
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
