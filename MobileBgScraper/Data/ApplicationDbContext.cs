using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileBgScraper.Data.Models;

namespace MobileBgScraper.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<VehiclePrice> VehiclePrices { get; set; }

        public DbSet<SearchUrl> SearchUrls { get; set; }

        public DbSet<UserSearchResult> UserSearchResults { get; set; }

        public DbSet<Specification> Specifications { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<VehicleHistoricPrice> VehicleHistoricPrices { get; set; }
    }
}
