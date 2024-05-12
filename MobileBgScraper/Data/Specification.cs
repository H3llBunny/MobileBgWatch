namespace MobileBgScraper.Data.Models
{
    public class Specification
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public virtual Vehicle Vehicle { get; set; }

        public string SpecificationName { get; set; }

        public string SpecificationValue { get; set; }
    }
}
