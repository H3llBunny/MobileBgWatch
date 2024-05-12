namespace MobileBgScraper.Data.Models
{
    public class UserSearchResult
    {
        public int Id { get; set; }

        public int SearchUrlId { get; set; }

        public virtual SearchUrl SearchUrl { get; set; }

        public int VehicleId { get; set; }

        public virtual Vehicle Vehicle { get; set; }
    }
}
