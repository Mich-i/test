using Microsoft.AspNetCore.SignalR.Client;

namespace ChatTool.Client.Services;

public class SignalingService
{
    private HubConnection? hub;

    public async Task<HubConnection> GetHub()
    {
        if (this.hub is { State: HubConnectionState.Disconnected })
        {
            await this.hub.StartAsync();
        }


        if (this.hub != null)
        {
            return this.hub;
        }

        this.hub = new HubConnectionBuilder()
            .WithUrl("https://localhost:7033/messagehub")
            .WithAutomaticReconnect()
            .Build();

        await this.hub.StartAsync();
        return this.hub;
    }
}