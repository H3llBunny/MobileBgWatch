using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBgWatch.Models;
using MobileBgWatch.Services;
using MobileBgWatch.ViewModels;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace MobileBgWatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUsersService _usersService;
        private readonly IScraperService _scraperService;
        private readonly IVehicleService _vehicleService;

        public HomeController(ILogger<HomeController> logger, IUsersService usersService, IScraperService scraperService, IVehicleService vehicleService)
        {
            _logger = logger;
            this._usersService = usersService;
            this._scraperService = scraperService;
            this._vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            SearchUrlsListViewModel searchUrlsViewModel = await this._vehicleService.GetSearchUrlsListAsync(userId, 40);

            return View(searchUrlsViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSearchUrl(string searchUrl, bool addToUser = true)
        {
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                ViewBag.ErrorMessage = "Please ensure the URL is valid and try again";
                return View("Index");
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await this._usersService.UserSearchUrlLimitAsync(userId))
            {
                ViewBag.ErrorMessage = "You have reached the limit of 5 search URLs. I might add a monetization to expand it later in the development.";
                return View("Index");
            }

            if (addToUser)
            {
                if (await this._usersService.SearchUrlAlreadyExist(userId, searchUrl))
                {
                    ViewBag.ErrorMessage = "Search Url already exist.";
                    return View("Index");
                }
            }

            try
            {
                var vehicleUrls = (await this._scraperService.GetAllVehicleAdUrlsAsync(searchUrl)).ToList();
                var vehicleList = await this._scraperService.CreateVehiclesListAsync(vehicleUrls, userId, searchUrl);
                await this._vehicleService.AddVehicleAsync(vehicleList);
                if (addToUser)
                {
                    await this._vehicleService.AddSearchUrlToUserAsync(userId, searchUrl);
                    TempData["SuccessMessage"] = "New search URL has been successfully added.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RefreshAds(string searchUrl)
        {
            return await AddSearchUrl(searchUrl, addToUser: false);
        }
    }
}
