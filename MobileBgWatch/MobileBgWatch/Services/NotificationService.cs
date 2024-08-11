using Microsoft.AspNetCore.SignalR;
using MobileBgWatch.Models;

namespace MobileBgWatch.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
        {
            this._logger = logger;
            this._hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            await this._hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
