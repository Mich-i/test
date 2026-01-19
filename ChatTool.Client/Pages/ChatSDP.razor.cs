using ChatTool.Client.Application;
using ChatTool.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ChatTool.Client.Pages;

public partial class ChatSDP
{
    [Parameter] public string LobbyCode { get; set; } = string.Empty;

    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public SignalingService SignalingService { get; set; } = null!;

    private HubConnection? hub;

    private string DataChannelState { get; set; } = "closed";
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
        this.hub.On<string>("PeerJoined", async _ =>
        {
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

        bool ok = await this.hub.InvokeAsync<bool>("JoinRoom", this.LobbyCode);
        if (!ok)
        {
            this.LogMessages.Add("JoinRoom failed (room not found or full).");
            this.StateHasChanged();
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

        // Serialize the object to JSON before sending to JS
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

        message!.userType = UserType.Peer;
        message.From = "Peer";
        this.Messages.Add(message);
        this.StateHasChanged();
    }

    [JSInvokable]
    public void DataChannelStateChanged(string state)
    {
        this.DataChannelState = state;
        this.LogMessages.Add($"DataChannel state: {state}");
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