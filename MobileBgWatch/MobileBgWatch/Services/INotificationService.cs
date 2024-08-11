namespace MobileBgWatch.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
    }
}
