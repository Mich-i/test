using Microsoft.AspNetCore.SignalR.Client;

namespace ChatTool.Client.Services;

public class SignalingService
{
    private HubConnection? hub;

    public async Task<HubConnection> GetHub()
    {
        if (this.hub != null)
        {
            return this.hub;
        }

        string hubUrl = "https://0.0.0.0:7033/messagehub";

        this.hub = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        await this.hub.StartAsync();
        return this.hub;
    }
}