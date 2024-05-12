namespace MobileBgScraper.Data.Models
{
    using System;

    public class VehicleHistoricPrice
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public virtual Vehicle Vehicle { get; set; }

        public int Price { get; set; }

        public string Currency { get; set; }

        public DateTime Date { get; set; }

        public bool IncludeVat { get; set; }
    }
}
