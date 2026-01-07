using ChatTool.Core.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace ChatTool.Server.Application.Hubs;

public class ChatHub : Hub
{
    private readonly UserInformation users;
    public ChatHub(UserInformation users)
    {
        this.users = users;
    }

    public override async Task OnConnectedAsync()
    {
        await this.Clients.Caller.SendAsync("ReceiveConnectionId", this.Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public Task<bool> SignUp(string name, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(name) || this.users.CheckIfTheUserAlreadyExist(name, connectionId))
        {
            return Task.FromResult(false);
        }

        this.users.AddUserToDictionary(name, connectionId);
        return Task.FromResult(true);
    }

    public Task<bool> SignIn(string name, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(name) || !this.users.CheckIfTheUserAlreadyExist(name, connectionId))
        {
            return Task.FromResult(false);
        }

        this.users.ChangeConnectionIdFromExistingUser(name, connectionId);
        return Task.FromResult(true);
    }

    public Task<string[]> GetLoggedInUsers()
    {
        return Task.FromResult(this.users.Users.Keys.ToArray());
    }

    public async Task<bool> SendPrivateToUser(string toUser, string fromUser, string message)
    {
        if (string.IsNullOrWhiteSpace(toUser) || string.IsNullOrWhiteSpace(fromUser) || string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        if (!this.users.Users.TryGetValue(toUser, out string? toConnectionId))
        {
            return false;
        }

        await this.Clients.Client(toConnectionId).SendAsync("ReceivePrivateMessage", fromUser, message);
        return true;
    }
}