using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MobileBgWatch.Hubs
{
    public class NotificationHub : Hub
    {
        private static ConcurrentDictionary<string, string> _connectedUsers = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            _connectedUsers.TryAdd(Context.UserIdentifier, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectedUsers.TryRemove(Context.UserIdentifier, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public static bool IsUserConnected(string userId)
        {
            return _connectedUsers.ContainsKey(userId);
        }
    }
}
