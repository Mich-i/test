using Microsoft.AspNetCore.Components.Web;
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
            HubConnection hub = await this.SignalingService.GetHub();
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

        string code = this.LobbyCode.Trim().ToUpperInvariant();

        try
        {
            HubConnection hub = await this.SignalingService.GetHub();

            bool canJoin = await hub.InvokeAsync<bool>("CanJoinRoom", code);
            if (!canJoin)
            {
                this.ErrorMessage = "Room not found or already full.";
                return;
            }

            this.NavigationManager.NavigateTo($"/chat/{code}");
        }
        catch (Exception exception)
        {
            this.ErrorMessage = exception.Message;
        }
    }

    private async Task SendOnKeyDown(KeyboardEventArgs pressedKey)
    {
        if (pressedKey.Key == "Enter")
        {
            await this.JoinRoom();
        }
    }
}