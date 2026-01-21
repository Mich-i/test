using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatTool.Server.Hubs;

public sealed class MessageHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> _roomCounts = new();

    public Task<string> CreateRoom()
    {
        string code = GenerateCode();
        code = NormalizeCode(code);

        _roomCounts[code] = 0;
        return Task.FromResult(code);
    }


    public async Task<bool> JoinRoom(string? code)
    {
        code = NormalizeCode(code);

        if (!_roomCounts.TryGetValue(code, out int count))
        {
            return false;
        }

        if (count >= 2)
        {
            return false;
        }

        _roomCounts[code] = count + 1;
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, code);

        await this.Clients.OthersInGroup(code).SendAsync("PeerJoined", code);

        return true;
    }

    public async Task SignalSdp(string? code, string type, string payload)
    {
        code = NormalizeCode(code);
        await this.Clients.OthersInGroup(code).SendAsync("SignalSdp", code, type, payload);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private static string NormalizeCode(string? code)
    {
        return (code ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 6).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    public Task<bool> CanJoinRoom(string? code)
    {
        code = NormalizeCode(code);

        if (!_roomCounts.TryGetValue(code, out int count))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(count < 2);
    }
}