using Microsoft.AspNetCore.SignalR;

namespace ChatTool.Server.Hubs
{
    public class MessageHub : Hub
    {
        public async Task Create(string channel)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channel);
            await Clients.OthersInGroup(channel).SendAsync("Create", Context.ConnectionId);
        }
        public async Task Join(string channel)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channel);
            await Clients.OthersInGroup(channel).SendAsync("Join", Context.ConnectionId);
        }
        public async Task Leave(string channel)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);
            await Clients.OthersInGroup(channel).SendAsync("Leave", Context.ConnectionId);
        }
    }
}
