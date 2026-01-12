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

    public async Task<string> CreateRoom()
    {
        string code = GenerateCode();

        logger.LogInformation("CreateRoom called by {ConnectionId}. Code={Code}", Context.ConnectionId, code);

        RoomCounts[code] = 1;
        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        return code;
    }

    public async Task<bool> JoinRoom(string? code)
    {
        logger.LogInformation("JoinRoom called by {ConnectionId}. Code={Code}", Context.ConnectionId, code);
        logger.LogInformation("RoomCounts has: {Rooms}", string.Join(",", RoomCounts.Keys));

        code = (code ?? string.Empty).Trim().ToUpperInvariant();

        if (!RoomCounts.TryGetValue(code, out int count))
            return false;

        if (count >= 2)
            return false;

        RoomCounts[code] = count + 1;
        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        // dem anderen sagen: "ein Peer ist beigetreten"
        await Clients.OthersInGroup(code).SendAsync("PeerJoined", code);

        return true;
    }

    public async Task SignalSdp(string code, string type, string payload)
    {
        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        await Clients.OthersInGroup(code).SendAsync("SignalSdp", code, type, payload);
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