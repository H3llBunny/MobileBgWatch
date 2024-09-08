using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MobileBgWatch.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<string>
    {
        public ApplicationUser()
        {
            this.SearchUrls = new List<SearchUrl>();
            this.ReceiveEmails = false;
        }

        public bool ReceiveEmails { get; set; }

        public ICollection<SearchUrl> SearchUrls { get; set; }
    }
}
