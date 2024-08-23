using Microsoft.AspNetCore.SignalR;
using MobileBgWatch.Hubs;

namespace MobileBgWatch.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            this._hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            if (NotificationHub.IsUserConnected(userId))
            {
                await this._hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
            }
        }
    }
}
