namespace MobileBgWatch.ViewModels
{
    public class SearchUrlsListViewModel
    {
        public SearchUrlsListViewModel()
        {
            this.SearchUrls = new List<SearchUrlViewModel>();
        }

        public ICollection<SearchUrlViewModel> SearchUrls { get; set; }
    }
}
