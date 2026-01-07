using ChatTool.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace ChatTool.Client.Pages;

public partial class ChatSDP
{
    [Parameter] public string LobbyCode { get; set; } = string.Empty;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public SignalingService SignalingService { get; set; } = null!;

    private HubConnection? hub;
    private bool isOfferer;

    private string DataChannelState { get; set; } = "closed";
    private string Message { get; set; } = string.Empty;
    private List<string> Messages { get; } = [];
    private List<string> LogMessages { get; } = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        // WebRTC initialisieren
        await this.InitWebRtc();

        this.hub = await this.SignalingService.GetHub();

        // Offer erstellen
        this.hub.On<string>("PeerJoined", async _ =>
        {
            this.isOfferer = true;

            this.LogMessages.Add("Peer joined → creating offer");

            string offer = await this.Js.InvokeAsync<string>("webRTCInterop.createOffer");
            await this.hub.InvokeAsync("SignalSdp", this.LobbyCode, "offer", offer);

            this.StateHasChanged();
        });

        // Offer / Answer empfangen
        this.hub.On<string, string, string>("SignalSdp", async (code, type, payload) =>
        {
            if (!string.Equals(code, this.LobbyCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            switch (type)
            {
                case "offer":
                {
                    this.isOfferer = false;
                    this.LogMessages.Add("Received offer → creating answer");

                    string answer = await this.Js.InvokeAsync<string>(
                        "webRTCInterop.receiveOfferAndCreateAnswer",
                        payload
                    );

                    await this.hub.InvokeAsync("SignalSdp", this.LobbyCode, "answer", answer);
                    break;
                }
                case "answer":
                    this.LogMessages.Add("Received answer → accepting");
                    await this.Js.InvokeAsync<bool>("webRTCInterop.receiveAnswer", payload);
                    break;
            }

            this.StateHasChanged();
        });

        // im Chatroom joinen
        await this.hub.InvokeAsync<bool>("JoinRoom", this.LobbyCode);
    }

    private async Task InitWebRtc()
    {
        await this.Js.InvokeVoidAsync("webRTCInterop.init",
            DotNetObjectReference.Create(this));
    }

    // Chat send
    private async Task OnSend()
    {
        if (string.IsNullOrWhiteSpace(this.Message))
        {
            return;
        }

        await this.Js.InvokeVoidAsync("webRTCInterop.sendMessage", this.Message);
        this.Messages.Add($"Me: {this.Message}");
        this.Message = string.Empty;
    }

    private async Task SendOnKeyDown(KeyboardEventArgs pressedKey)
    {
        if (pressedKey.Key == "Enter")
        {
            await this.OnSend();
        }
    }

    [JSInvokable]
    public void OnMessageReceived(string message)
    {
        this.Messages.Add($"Peer: {message}");
        this.StateHasChanged();
    }

    [JSInvokable]
    public void OnDataChannelStateChanged(string state)
    {
        this.DataChannelState = state;
        this.LogMessages.Add($"DataChannel state: {state}");
        this.StateHasChanged();
    }
}