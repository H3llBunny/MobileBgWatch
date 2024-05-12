namespace MobileBgScraper.Data.Models
{
    public class Image
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public virtual Vehicle Vehicle { get; set; }

        public string ImageUrl { get; set; }
    }
}
