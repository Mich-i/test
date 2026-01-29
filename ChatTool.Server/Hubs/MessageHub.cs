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
        if (!_roomCounts.ContainsKey(code)) return false;

        _roomCounts[code]++;
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, code);

        // Sende die ID des neuen Peers an alle anderen
        await this.Clients.OthersInGroup(code).SendAsync("PeerJoined", this.Context.ConnectionId);
        return true;
    }

    public async Task SignalSdp(string code, string targetConnectionId, string type, string payload)
    {
        await this.Clients.Client(targetConnectionId).SendAsync("SignalSdp", this.Context.ConnectionId, type, payload);
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
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}