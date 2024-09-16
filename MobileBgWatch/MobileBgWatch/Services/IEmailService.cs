using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, List<Vehicle> newVehicleAds);
    }
}
