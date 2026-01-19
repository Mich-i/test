using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace ChatTool.Client.Services;

public sealed class SignalingService
{
    private readonly NavigationManager navigationManager;
    private readonly IConfiguration configuration;

    private HubConnection? hubConnection;

    public SignalingService(NavigationManager navigationManager, IConfiguration configuration)
    {
        this.navigationManager = navigationManager;
        this.configuration = configuration;
    }

    public async Task<HubConnection> GetHub()
    {
        if (this.hubConnection != null)
        {
            return this.hubConnection;
        }

        string hubPath = this.configuration["Signaling:HubPath"] ?? "/messagehub";
        if (!hubPath.StartsWith("/")) hubPath = "/" + hubPath;

        // Try same-origin absolute URL first
        string sameOriginUrl = this.navigationManager.ToAbsoluteUri(hubPath).ToString();
        Console.WriteLine($"[SignalingService] Trying same-origin HubUrl = {sameOriginUrl}");

        this.hubConnection = new HubConnectionBuilder()
            .WithUrl(sameOriginUrl)
            .WithAutomaticReconnect()
            .Build();

        try
        {
            await this.hubConnection.StartAsync();
            return this.hubConnection;
        }
        catch (Exception firstEx)
        {
            Console.WriteLine($"[SignalingService] First attempt failed: {firstEx.Message}. Trying configured server address...");

            try { await this.hubConnection.DisposeAsync(); } catch { }
            this.hubConnection = null;

            string configuredUrl = this.BuildConfiguredHubUrl(hubPath);
            Console.WriteLine($"[SignalingService] Configured HubUrl = {configuredUrl}");

            this.hubConnection = new HubConnectionBuilder()
                .WithUrl(configuredUrl)
                .WithAutomaticReconnect()
                .Build();

            try
            {
                await this.hubConnection.StartAsync();
                return this.hubConnection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalingService] Failed to start hub connection (configured url): {ex}");
                throw;
            }
        }
    }

    private string BuildConfiguredHubUrl(string hubPath)
    {
        string serverScheme = this.configuration["Signaling:ServerScheme"] ?? "https";
        string? serverPortText = this.configuration["Signaling:ServerPort"];

        var currentUri = new Uri(this.navigationManager.Uri);
        string hostName = currentUri.Host;

        int serverPort;
        if (!int.TryParse(serverPortText, out serverPort))
        {
            int pagePort = currentUri.IsDefaultPort ? (serverScheme == "https" ? 443 : 80) : currentUri.Port;
            Console.WriteLine($"[SignalingService] Warning: Signaling:ServerPort missing or invalid. Falling back to page port {pagePort}.");
            serverPort = pagePort;
        }

        var hubUri = new UriBuilder(serverScheme, hostName, serverPort, hubPath);
        return hubUri.Uri.ToString();
    }
}