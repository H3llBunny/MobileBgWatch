using AngleSharp;
using Microsoft.AspNetCore.Identity;
using MobileBgWatch.Hubs;
using MobileBgWatch.Models;
using MobileBgWatch.Profiles;
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

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
                .AddMongoDbStores<ApplicationUser, ApplicationRole, string>(mongoDbSettings.ConnectionString, mongoDbSettings.Database)
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/User/Login";
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

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

            builder.Services.AddSingleton<IMongoCollection<UserForEmailing>>(sp =>
            {
                var mongoDbConfig = sp.GetRequiredService<MongoDbConfig>();
                var client = new MongoClient(mongoDbConfig.ConnectionString);
                var database = client.GetDatabase(mongoDbConfig.Database);
                return database.GetCollection<UserForEmailing>("UsersForEmailing");
            });

            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IScraperService, ScraperService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<ISearchUrlService, SearchUrlService>();
            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGridOptions"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IReceiveEmailsService, ReceiveEmailsService>();

            builder.Services.AddSignalR();

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddHostedService<AutoRefreshAdsService>();

            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.MapHub<NotificationHub>("/notificationHub");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
