using Microsoft.AspNetCore.SignalR.Client;

namespace ChatTool.Client.Services;

public class SignalingService
{
    private HubConnection? hub;

    public async Task<HubConnection> GetHub()
    {
        if (this.hub is not null)
        {
            return this.hub;
        }

        this.hub = new HubConnectionBuilder()
            .WithUrl("https://localhost:50783/messagehub")
            .WithAutomaticReconnect()
            .Build();

        await this.hub.StartAsync();
        return this.hub;
    }
}