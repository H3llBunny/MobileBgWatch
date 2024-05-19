using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MobileBgWatch.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<string>
    {
        public ApplicationUser()
        {
            this.SearchUrls = new List<string>();
        }

        public ICollection<string> SearchUrls { get; set; }
    }
}
