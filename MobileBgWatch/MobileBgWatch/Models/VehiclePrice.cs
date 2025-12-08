namespace MobileBgWatch.Models
{
    public class VehiclePrice
    {
        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime Date { get; set; }

        public bool IncludeVat { get; set; }
    }
}
