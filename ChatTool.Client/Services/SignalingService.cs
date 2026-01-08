using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ChatTool.Client.Services;

public class SignalingService(NavigationManager navigationManager, IConfiguration configuration)
{
    private HubConnection? hub;

    public async Task<HubConnection> GetHub()
    {
        if (this.hub != null)
        {
            return this.hub;
        }

        string hubUrl = "https://localhost:7000/messagehub";
        this.hub = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        await this.hub.StartAsync();
        return this.hub;
    }
}