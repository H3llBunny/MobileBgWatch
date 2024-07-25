namespace MobileBgWatch.Models
{
    public class SearchUrl
    {
        public string Url { get; set; }

        public DateTime LastRefreshByUser { get; set; }

        public DateTime LastRefreshByService { get; set; }

        public int RefreshCounter { get; set; }
    }
}
