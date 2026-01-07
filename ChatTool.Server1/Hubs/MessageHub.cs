using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatTool.Server.Hubs;

public class MessageHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> _roomCounts = new();

    public async Task<string> CreateRoom()
    {
        string code = GenerateCode();

        _roomCounts[code] = 1;
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, code);

        // Host Code sagen
        await this.Clients.Caller.SendAsync("RoomCreated", code);
        return code;
    }

    public async Task<bool> JoinRoom(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        code = code.Trim().ToUpperInvariant();

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

        // Host informieren
        await this.Clients.OthersInGroup(code).SendAsync("PeerJoined", this.Context.ConnectionId);

        return true;
    }

    public async Task LeaveRoom(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        code = code.Trim().ToUpperInvariant();

        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, code);

        if (_roomCounts.TryGetValue(code, out int count))
        {
            int newCount = count - 1;
            if (newCount <= 0)
            {
                _roomCounts.TryRemove(code, out _);
            }
            else
            {
                _roomCounts[code] = newCount;
            }
        }

        await this.Clients.OthersInGroup(code).SendAsync("PeerLeft", this.Context.ConnectionId);
    }

    // Singaling
    public async Task SignalSdp(string code, string type, string payload)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        code = code.Trim().ToUpperInvariant();

        // Code senden
        await this.Clients.OthersInGroup(code).SendAsync("SignalSdp", code, type, payload);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = Random.Shared;

        while (true)
        {
            string code = new(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
            if (!_roomCounts.ContainsKey(code))
            {
                return code;
            }
        }
    }
}