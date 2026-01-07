using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

namespace ChatTool.Client.Services;

public class SignalingService(NavigationManager navigationManager)
{
    private HubConnection? hub;

    public async Task<HubConnection> GetHub()
    {
        if (this.hub != null)
        {
            return this.hub;
        }

        this.hub = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/messagehub"))
            .Build();

        await this.hub.StartAsync();
        return this.hub;
    }
}