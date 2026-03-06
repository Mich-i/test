using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ChatTool.Desktop.Services;

public sealed class SignalingService
{
    private readonly IConfiguration configuration;
    private HubConnection? hubConnection;

    public SignalingService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<HubConnection> GetHub()
    {
        if (this.hubConnection != null)
        {
            return this.hubConnection;
        }

        string hubUrl = this.configuration["Signaling:HubUrl"]!;

        Console.WriteLine($"[SignalingService] Connecting to: {hubUrl}");

        this.hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        await this.hubConnection.StartAsync();

        Console.WriteLine("[SignalingService] Connected");

        return this.hubConnection;
    }
}