using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ChatTool.Client.Pages;

public partial class ChatSDP
{
    private string? LocalSdp { get; set; }
    private string? RemoteSdp { get; set; }
    private List<string> Messages { get; set; } = new();
    private List<string> LogMessages { get; set; } = new();
    private string? Message { get; set; }
    public string DataChannelState { get; set; } = "closed";
    private DotNetObjectReference<ChatSDP>? objRef;

    protected override void OnInitialized()
    {
        this.objRef?.Dispose();
        this.objRef = DotNetObjectReference.Create(this);
    }

    public async Task Init()
    {
        await this.JS.InvokeVoidAsync("webRTCInterop.initialize", this.objRef);
        this.LogMessages.Add("Initialized webRTCInterop");
    }

    public async Task CreateOffer()
    {
        string local = await this.JS.InvokeAsync<string>("webRTCInterop.createOffer");
        this.LocalSdp = local;
        this.LogMessages.Add("Offer created. Copy Local SDP to remote peer.");
    }

    public async Task AcceptOffer()
    {
        if (string.IsNullOrWhiteSpace(this.RemoteSdp))
        {
            this.LogMessages.Add("Paste remote offer SDP first.");
            return;
        }

        string? answer = await this.JS.InvokeAsync<string>("webRTCInterop.receiveOfferAndCreateAnswer", this.RemoteSdp);
        this.LocalSdp = answer;
        this.LogMessages.Add("Answer created. Copy Local SDP back to offerer.");
    }

    public async Task AcceptAnswer()
    {
        if (string.IsNullOrWhiteSpace(this.RemoteSdp))
        {
            this.LogMessages.Add("Paste remote answer SDP first.");
            return;
        }

        bool ok = await this.JS.InvokeAsync<bool>("webRTCInterop.receiveAnswer", this.RemoteSdp);
        this.LogMessages.Add(ok ? "Remote answer accepted." : "Failed to accept remote answer.");
    }

    public async Task OnSend()
    {
        if (string.IsNullOrWhiteSpace(this.Message)) return;

        bool isOpen = await this.JS.InvokeAsync<bool>("webRTCInterop.isDataChannelOpen");
        if (!isOpen)
        {
            this.Messages.Add("Data channel not open — message queued locally.");
        }

        bool sent = await this.JS.InvokeAsync<bool>("webRTCInterop.sendData", this.Message);
        if (sent) this.Messages.Add($"Me: {this.Message}");
        else if (!isOpen) this.Messages.Add($"Queued: {this.Message}");
        else this.Messages.Add("Send failed — data channel not open.");

        this.Message = string.Empty;
    }

    [JSInvokable]
    public void ReceiveMessage(string message)
    {
        this.Messages.Add($"Peer: {message}");
        this.StateHasChanged();
    }

    [JSInvokable]
    public void DataChannelStateChanged(string state)
    {
        this.DataChannelState = state;
        this.LogMessages.Add($"Data channel: {state}");
        this.StateHasChanged();
    }

    public async Task Close()
    {
        await this.JS.InvokeVoidAsync("webRTCInterop.close");
        this.Messages.Add("Closed connection");
        this.DataChannelState = "closed";
    }

    public void Dispose() => this.objRef?.Dispose();

    private async Task SendOnKeyDown(KeyboardEventArgs pressedKey) 
    {
        if (pressedKey.Key == "Enter")
        {
            await this.OnSend();
        }
    }
}