using AngleSharp;
using AngleSharp.Dom;
using MobileBgWatch.Models;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MobileBgWatch.Services
{
    public class ScraperService : IScraperService
    {
        private IBrowsingContext _context;

        public ScraperService(IBrowsingContext context, IVehicleService vehicleService)
        {
            this._context = context;
        }

        public async Task<IEnumerable<string>> GetAllVehicleAdUrlsAsync(string searchUrl, string userId, bool shortScrape)
        {
            var vehicleUrls = new List<string>();

            string initialUrl = searchUrl;

            try
            {
                if (shortScrape)
                {
                    initialUrl = initialUrl.Replace("sort=6", "sort=7");
                }

                var initialDocument = await this._context.OpenAsync(initialUrl);

                if (initialDocument == null)
                {
                    throw new Exception("Failed to load the initial document. Please ensure the URL is valid and try again.");
                }

                int totalPages = 0;
                var paginationElement = initialDocument.QuerySelector("div.pagination a.saveSlink.gray");
                if (paginationElement != null)
                {
                    int.TryParse(paginationElement.TextContent.Trim(), out totalPages);
                }
                else
                {
                    var paginationElements = initialDocument.QuerySelectorAll("div.pagination a.saveSlink");
                    if (paginationElements != null && paginationElements.Any())
                    {
                        totalPages = paginationElements
                            .Where(x => int.TryParse(x.TextContent, out _))
                            .Select(x => int.Parse(x.TextContent))
                            .DefaultIfEmpty(0)
                            .Max();
                    }
                }

                if (totalPages < 1)
                {
                    throw new Exception("No pages found. Please ensure the URL is valid and try again.");
                }

                for (int page = 1; page <= totalPages; page++)
                {
                    string currentPageUrl = initialUrl.Contains("?")
                        ? initialUrl.Insert(initialUrl.IndexOf("?"), $"/p-{page}")
                        : initialUrl + $"/p-{page}";

                    var currentPageDocument = await _context.OpenAsync(currentPageUrl);
                    if (currentPageDocument == null)
                    {
                        continue;
                    }

                    var vehicleAdDivs = currentPageDocument.QuerySelectorAll("div.item, div.item.TOP, div.item.VIP");

                    foreach (var div in vehicleAdDivs)
                    {
                        var link = div.QuerySelector("div.links a");
                        string fullUrl = "https:" + link.GetAttribute("href");

                        vehicleUrls.Add(fullUrl);
                    }
                }

                return vehicleUrls;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while processing the URL: " + ex.Message);
            }
        }

        public async Task<ICollection<Vehicle>> CreateVehiclesListAsync(IEnumerable<string> vehicleUrls, string userId, string searchUrl)
        {
            var vehicleList = new List<Vehicle>();

            try
            {
                foreach (var url in vehicleUrls)
                {
                    var document = await this._context.OpenAsync(url);
                    var nameElement = document.QuerySelector("div.obTitle h1");
                    if (nameElement == null)
                    {
                        continue;
                    }

                    string name = nameElement.FirstChild.TextContent.Trim();
                    var secondPartOfName = document.QuerySelector("div.obTitle span");
                    if (secondPartOfName != null)
                    {
                        name += $" - {secondPartOfName.TextContent.Trim()}";
                    }

                    long vehicleAdId = long.Parse(document.QuerySelector("div.obiava").TextContent.Trim().Where(char.IsDigit).ToArray());
                    string location = document.QuerySelector("div.carLocation span").Text();
                    bool vatIncluded = false;
                    var vatCheck = document.QuerySelector("div.PriceInfo");
                    if (vatCheck != null)
                    {
                        vatIncluded = vatCheck.Text() == "Цената е с включено ДДС" ? true : false;
                    }

                    int price = 0;
                    string currency = "N/A";
                    var priceElement = document.QuerySelector("div.Price");
                    if (priceElement != null)
                    {
                        var priceText = priceElement.FirstChild?.TextContent.Trim();
                        if (!string.IsNullOrWhiteSpace(priceText))
                        {
                            var priceDigits = new string(priceText.Where(char.IsDigit).ToArray());
                            if (!string.IsNullOrWhiteSpace(priceDigits))
                            {
                                price = int.Parse(priceDigits);
                                var currencyParts = priceText.Split(' ');
                                if (currencyParts.Length > 1)
                                {
                                    currency = currencyParts.Last().Trim();
                                }
                            }
                        }
                    }
                    var currentPrice = new VehiclePrice
                    {
                        Price = price,
                        Currency = currency,
                        Date = DateTime.UtcNow,
                        IncludeVat = vatIncluded
                    };

                    var specifications = new Dictionary<string, string>();
                    var itemElements = document.QuerySelectorAll("div.techData div.item");
                    foreach (var itemElement in itemElements)
                    {
                        string key = itemElement.Children[0].Text();
                        string value = itemElement.Children[1].Text();
                        specifications.Add(key, value);
                    }

                    var imgUrlElements = document.QuerySelectorAll("img.carouselimg");
                    var imgUrls = new List<string>();
                    if (imgUrlElements.Length > 0)
                    {
                        foreach (var imgUrlElement in imgUrlElements)
                        {
                            imgUrls.Add(imgUrlElement.GetAttribute("data-src-gallery"));
                        }
                    }
                    else
                    {
                        imgUrls.Add("/images/noImage.jpg");
                    }

                    var vehicle = new Vehicle
                    {
                        UserId = userId,
                        SearchUrl = searchUrl,
                        VehicleAdId = vehicleAdId,
                        Name = name,
                        DateAdded = DateTime.UtcNow,
                        CurrentPrice = currentPrice,
                        PreviousPrice = new VehiclePrice { Price = 0, Currency = currentPrice.Currency, Date = DateTime.UtcNow, IncludeVat = currentPrice.IncludeVat },
                        Url = url,
                        Location = location,
                        Specifications = specifications,
                        ImageUrls = imgUrls,
                        Favorite = false
                    };

                    vehicleList.Add(vehicle);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return vehicleList;
        }


        public string UpdateSortParameter(string searchUrl)
        {
            string sortPattern = @"([?&])sort=\d+";
            string replacement = "$1sort=6";

            if (Regex.IsMatch(searchUrl, sortPattern))
            {
                return Regex.Replace(searchUrl, sortPattern, replacement);
            }
            else
            {
                if (searchUrl.Contains("?"))
                {
                    return searchUrl + "&sort=6";
                }
                else
                {
                    return searchUrl + "?sort=6";
                }
            }
        }
    }
}
