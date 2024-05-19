using AngleSharp;
using AngleSharp.Dom;
using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Services
{
    public class ScraperService : IScraperService
    {
        private IBrowsingContext context;

        public ScraperService()
        {
            this.context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        }

        public async Task<IEnumerable<string>> GetAllVehicleAdUrlsAsync(string searchUrl)
        {
            var vehicleUrls = new List<string>();
            string initialUrl = searchUrl;
            
            var initialDocument = await this.context.OpenAsync(initialUrl);

            int totalPages = 0;
            var paginationElement = initialDocument.QuerySelector("div.pagination a.saveSlink.gray");
            if (paginationElement != null)
            {
                int.TryParse(paginationElement.TextContent.Trim(), out totalPages);
            }
            else
            {
                var paginationElements = initialDocument.QuerySelectorAll("div.pagination a.saveSlink");
                totalPages = paginationElements
                    .Where(x => int.TryParse(x.TextContent, out _))
                    .Select(x => int.Parse(x.TextContent))
                    .Max();
            }

            if (totalPages >= 1)
            {
                for (int page = 1; page <= totalPages; page++)
                {
                    string currentPageUrl;

                    if (initialUrl.Contains("?"))
                    {
                        int substringIndex = initialUrl.IndexOf("?");
                        currentPageUrl = initialUrl.Insert(substringIndex, $"/p-{page}");
                    }
                    else
                    {
                        currentPageUrl = initialUrl + $"/p-{page}";
                    }

                    var currentPageDocument = await context.OpenAsync(currentPageUrl);

                    var vehicleAdLinks = currentPageDocument.QuerySelectorAll("div.photo div.big a");

                    foreach (var link in vehicleAdLinks)
                    {
                        string href = link.GetAttribute("href");
                        string fullUrl = "https:" + href;
                        vehicleUrls.Add(fullUrl);
                    }
                }

                return vehicleUrls;
            }
            else
            {
                throw new Exception("Please ensure the URL is valid and try again.");
            }
        }

        public async Task<ICollection<Vehicle>> CreateVehiclesListAsync(IEnumerable<string> vehicleUrls, string userId, string searchUrl)
        {
            var vehicleList = new List<Vehicle>();

            foreach (var url in vehicleUrls)
            {
                var document = await this.context.OpenAsync(url);
                string name = document.QuerySelector("div.obTitle").FirstChild.Text().Trim();
                var secondPartOfName = document.QuerySelector("div.obTitle span");
                if (secondPartOfName != null)
                {
                    name += $" - {secondPartOfName.TextContent.Trim()}";
                }
                long vehicleAdId = long.Parse(document.QuerySelector("div.obiava").TextContent.Trim().Where(char.IsDigit).ToArray());
                string location = document.QuerySelector("div.carLocation span").Text();
                bool vatIncluded = false;
                if (document.QuerySelector("div.PriceInfo").Text() == "Цената е с включено ДДС")
                {
                    vatIncluded = true;
                }
                int price = int.Parse(document.QuerySelector("div.Price").FirstChild.TextContent.Trim().Where(char.IsDigit).ToArray());
                string currency = document.QuerySelector("div.Price").FirstChild.TextContent.Trim().Split().Last();
                var currentPrice = new VehiclePrice
                {
                    Price = price,
                    Currency = currency,
                    Date = DateTime.Today,
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
                foreach (var imgUrlElement in imgUrlElements)
                {
                    imgUrls.Add("https:" + imgUrlElement.GetAttribute("data-src-gallery"));
                }

                var vehicle = new Vehicle
                {
                    UserId = userId,
                    SearchUrl = searchUrl,
                    VehicleAdId = vehicleAdId,
                    Name = name,
                    DateAdded = DateTime.Today,
                    CurrentPrice = currentPrice,
                    PreviousPrice = null,
                    Url = url,
                    Location = location,
                    Specifications = specifications,
                    ImageUrls = imgUrls,
                };

                vehicleList.Add(vehicle);
            }

            return vehicleList;
        }
    }
}
