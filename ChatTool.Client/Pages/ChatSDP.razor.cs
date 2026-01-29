using ChatTool.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Text.Json;
using ChatTool.Shared;

namespace ChatTool.Client.Pages;

public partial class ChatSDP
{
    [Parameter] public string LobbyCode { get; set; } = string.Empty;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public SignalingService SignalingService { get; set; } = null!;

    private HubConnection? hub;

    private HashSet<string> ConnectedPeers { get; } = new();
    private bool IsAnyConnectionOpen => this.ConnectedPeers.Any();
    private string Message { get; set; } = string.Empty;
    private string Name { get; set; } = "Me";
    private List<ChatMessage> Messages { get; } = [];
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
        // Wenn ein neuer Peer joint, erstelle ich ein Offer SPEZIELL für ihn
        this.hub.On<string>("PeerJoined", async (peerConnectionId) =>
        {
            this.LogMessages.Add($"New Peer {peerConnectionId} joined. Creating offer...");

            string offer = await this.Js.InvokeAsync<string>("webRTCInterop.createOffer", peerConnectionId);

           await this.hub.InvokeAsync("SignalSdp", this.LobbyCode, peerConnectionId, "offer", offer);
            this.StateHasChanged();
        });

        // Empfang von SDP (Offer oder Answer)
        this.hub.On<string, string, string>("SignalSdp", async (senderId, type, payload) =>
        {
            if (type == "offer")
            {
                this.LogMessages.Add($"Received offer from {senderId}");
                string answer = await this.Js.InvokeAsync<string>("webRTCInterop.receiveOfferAndCreateAnswer", senderId, payload);
                await this.hub.InvokeAsync("SignalSdp", this.LobbyCode, senderId, "answer", answer);
            }
            else if (type == "answer")
            {
                this.LogMessages.Add($"Received answer from {senderId}");
                await this.Js.InvokeAsync<bool>("webRTCInterop.receiveAnswer", senderId, payload);
            }
            this.StateHasChanged();
        });

        this.LobbyCode = this.LobbyCode.Trim().ToUpperInvariant();

        bool joined = await this.hub.InvokeAsync<bool>("JoinRoom", this.LobbyCode);
        if (!joined)
        {
            this.LogMessages.Add("Room not found or already full.");
            this.StateHasChanged();
            return;
        }
    }

    private async Task InitWebRtc()
    {
        await this.Js.InvokeAsync<bool>(
            "webRTCInterop.initialize",
            DotNetObjectReference.Create(this));
    }

    private async Task OnSend()
    {
        if (string.IsNullOrWhiteSpace(this.Message))
        {
            return;
        }

        ChatMessage chatMessage = new(this.Message, this.Name, UserType.Me);

        string json = JsonSerializer.Serialize(chatMessage);

        bool sent = await this.Js.InvokeAsync<bool>("webRTCInterop.sendData", json);

        if (!sent)
        {
            this.LogMessages.Add("Send failed: DataChannel not open.");
            return;
        }

        this.Messages.Add(chatMessage);
        this.Message = string.Empty;
    }

    [JSInvokable]
    public void ReceiveMessage(string json)
    {
        ChatMessage? message = JsonSerializer.Deserialize<ChatMessage>(json);

        message?.UpdateUserTypeAndSender();

        if (message != null) this.Messages.Add(message);
        this.StateHasChanged();
    }

    [JSInvokable]
    public void DataChannelStateChanged(string stateUpdate)
    {
        var parts = stateUpdate.Split(": ");
        if (parts.Length == 2)
        {
            string peerId = parts[0];
            string state = parts[1];

            if (state == "open")
                this.ConnectedPeers.Add(peerId);
            else
                this.ConnectedPeers.Remove(peerId);

        }

        this.LogMessages.Add($"Update: {stateUpdate}");
        this.StateHasChanged();
    }

    private async Task SendOnKeyDown(KeyboardEventArgs pressedKey)
    {
        if (pressedKey.Key == "Enter")
        {
            await this.OnSend();
        }
    }
}