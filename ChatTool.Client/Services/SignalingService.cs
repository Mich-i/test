using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ChatTool.Client.Services;

public class SignalingService
{
    private readonly NavigationManager navigationManager;
    private readonly IConfiguration configuration;

    private HubConnection? hub;

    public SignalingService(NavigationManager navigationManager, IConfiguration configuration)
    {
        this.navigationManager = navigationManager;
        this.configuration = configuration;
    }

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