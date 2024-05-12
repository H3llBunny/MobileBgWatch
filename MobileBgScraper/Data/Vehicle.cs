namespace MobileBgScraper.Data.Models
{
    using System.Collections.Generic;

    public class Vehicle
    {
        public Vehicle()
        {
            this.Specifications = new HashSet<Specification>();
            this.ImageUrls = new HashSet<Image>();
            this.HistoricalPrices = new List<VehicleHistoricPrice>();
            this.UserSearchResults = new HashSet<UserSearchResult>();
        }
        
        public int Id { get; set; }

        public int VehicleAdId { get; set; }

        public string Name { get; set; }

        public int VehcilePriceId { get; set; }

        public VehiclePrice Price { get; set; }

        public string Url { get; set; }

        public string Location { get; set; }

        public virtual ICollection<Specification> Specifications { get; set; }

        public virtual ICollection<Image> ImageUrls { get; set; }

        public virtual ICollection<VehicleHistoricPrice> HistoricalPrices { get; set; }

        public virtual ICollection<UserSearchResult> UserSearchResults { get; set; }
    }
}
