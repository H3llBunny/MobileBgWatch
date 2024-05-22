using AngleSharp;
using MobileBgWatch.Models;
using MobileBgWatch.Services;
using MongoDB.Driver;

namespace MobileBgWatch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<MongoDbConfig>(sp =>
            {
                var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                return configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
            });

            var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

            var identityBuilder = builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
            })
                .AddMongoDbStores<ApplicationUser, ApplicationRole, string>(mongoDbSettings.ConnectionString, mongoDbSettings.Database);

            builder.Services.AddSingleton<IBrowsingContext>(provider =>
            {
                var config = Configuration.Default.WithDefaultLoader();
                return BrowsingContext.New(config);
            });

            builder.Services.AddSingleton<IMongoCollection<ApplicationUser>>(sp =>
            {
                var mongoDbConfig = sp.GetRequiredService<MongoDbConfig>();
                var client = new MongoClient(mongoDbConfig.ConnectionString);
                var database = client.GetDatabase(mongoDbConfig.Database);
                return database.GetCollection<ApplicationUser>("Users");
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IScraperService, ScraperService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
