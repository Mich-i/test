using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ChatTool.Client.Services;

public sealed class SignalingService
{
    private readonly NavigationManager navigationManager;
    private readonly IConfiguration configuration;

    private HubConnection? hubConnection;

    public SignalingService(
        NavigationManager navigationManager,
        IConfiguration configuration)
    {
        this.navigationManager = navigationManager;
        this.configuration = configuration;
        Console.WriteLine(configuration);
    }

    public async Task<HubConnection> GetHub()
    {
        if (this.hubConnection != null)
        {
            return this.hubConnection;
        }

        string hubUrl = this.BuildHubUrl();

        this.hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        await this.hubConnection.StartAsync();

        return this.hubConnection;
    }

    private string BuildHubUrl()
    {
        string hubPath = this.configuration["Signaling:HubPath"]
            ?? throw new InvalidOperationException("Missing config: Signaling:HubPath");

        string scheme = this.configuration["Signaling:ServerScheme"]
            ?? throw new InvalidOperationException("Missing config: Signaling:ServerScheme");

        string portText = this.configuration["Signaling:ServerPort"]
            ?? throw new InvalidOperationException("Missing config: Signaling:ServerPort");

        if (!int.TryParse(portText, out int port))
        {
            throw new InvalidOperationException(
                $"Invalid config: Signaling:ServerPort = '{portText}'");
        }

        string? configuredHost = this.configuration["Signaling:ServerHost"];

        string host;
        if (!string.IsNullOrWhiteSpace(configuredHost))
        {
            host = configuredHost;
            Console.WriteLine($"[SignalingService] Using configured host: {host}");
        }
        else
        {
            host = new Uri(this.navigationManager.Uri).Host;
            Console.WriteLine($"[SignalingService] Using browser host: {host}");
        }

        UriBuilder uriBuilder = new()
        {
            Scheme = scheme,
            Host = host,
            Port = port,
            Path = hubPath,
        };

        return uriBuilder.Uri.ToString();

    }
}
