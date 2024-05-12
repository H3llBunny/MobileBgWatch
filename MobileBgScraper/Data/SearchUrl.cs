namespace MobileBgScraper.Data.Models
{
    using System.Collections.Generic;

    public class SearchUrl
    {
        public SearchUrl()
        {
            this.UserSearchResults = new HashSet<UserSearchResult>();
        }
        public int Id { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public virtual ICollection<UserSearchResult> UserSearchResults { get; set; }
    }
}
