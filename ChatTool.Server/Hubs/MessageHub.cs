using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatTool.Server.Hubs;

public class MessageHub : Hub
{
    private readonly ILogger<MessageHub> logger;

    public MessageHub(ILogger<MessageHub> logger)
    {
        this.logger = logger;
    }

    private static readonly ConcurrentDictionary<string, int> RoomCounts = new();

    public Task<string> CreateRoom()
    {
        string code = GenerateCode();

        this.logger.LogInformation("CreateRoom called by {ConnectionId}. Code={Code}", this.Context.ConnectionId, code);

        // Room existiert, aber noch niemand ist beigetreten
        RoomCounts[code] = 0;

        return Task.FromResult(code);
    }


    public async Task<bool> JoinRoom(string? code)
    {
        this.logger.LogInformation("JoinRoom called by {ConnectionId}. Code={Code}", this.Context.ConnectionId, code);
        this.logger.LogInformation("RoomCounts has: {Rooms}", string.Join(",", RoomCounts.Keys));

        code = (code ?? string.Empty).Trim().ToUpperInvariant();

        if (!RoomCounts.TryGetValue(code, out int count))
        {
            return false;
        }

        if (count >= 2)
        {
            return false;
        }

        RoomCounts[code] = count + 1;
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, code);

        // sagen: "ein Peer ist beigetreten"
        await this.Clients.OthersInGroup(code).SendAsync("PeerJoined", code);

        return true;
    }

    public async Task SignalSdp(string code, string type, string payload)
    {
        code = (code).Trim().ToUpperInvariant();
        await this.Clients.OthersInGroup(code).SendAsync("SignalSdp", code, type, payload);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = Random.Shared;

        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}