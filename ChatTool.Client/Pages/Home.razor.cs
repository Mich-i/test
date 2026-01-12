using Microsoft.AspNetCore.SignalR.Client;

namespace ChatTool.Client.Pages;

public partial class Home
{
    private string? LobbyCode { get; set; }
    private string? ErrorMessage { get; set; }

    private async Task CreateRoom()
    {
        try
        {
            var hub = await this.SignalingService.GetHub();
            string code = await hub.InvokeAsync<string>("CreateRoom");
            this.NavigationManager.NavigateTo($"/chat/{code}");
        }
        catch (Exception exception)
        {
            this.ErrorMessage = exception.Message;
        }
    }

    private async Task JoinRoom()
    {
        this.ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(this.LobbyCode))
        {
            this.ErrorMessage = "Please enter a lobby code to join.";
            return;
        }

        var hub = await this.SignalingService.GetHub();
        bool ok = await hub.InvokeAsync<bool>("JoinRoom", this.LobbyCode);

        if (!ok)
        {
            this.ErrorMessage = "Room not found or already full.";
            return;
        }

        this.NavigationManager.NavigateTo($"/chat/{this.LobbyCode.Trim().ToUpperInvariant()}");
    }
}