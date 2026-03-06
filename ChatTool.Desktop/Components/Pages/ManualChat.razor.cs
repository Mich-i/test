using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ChatTool.Shared;

namespace ChatTool.Desktop.Components.Pages
{
    public partial class ManualChat : IDisposable
    {
        private string? LocalSdp { get; set; }
        private string? RemoteSdp { get; set; }

        private List<ChatMessage> Messages { get; set; } = [];

        private string? Message { get; set; }
        private string DataChannelState { get; set; } = "closed";

        private DotNetObjectReference<ManualChat>? objRef;

        public ManualChat(IJSRuntime jsManual)
        {
            this.JsManual = jsManual;
        }

        [Inject]
        public IJSRuntime JsManual { get; set; }

        protected override void OnInitialized()
        {
            this.objRef?.Dispose();
            this.objRef = DotNetObjectReference.Create(this);
        }

        public async Task Init()
        {
            await this.JsManual.InvokeVoidAsync("manualWebRTC.initialize", this.objRef);

            this.Messages.Add(new ChatMessage("Bereit. Bitte Initialisieren klicken.", "System", UserType.Peer));
        }

        public async Task CreateOffer()
        {
            this.Messages.Add(new ChatMessage("Erstelle Offer...", "System", UserType.Peer));
            this.LocalSdp = await this.JsManual.InvokeAsync<string>("manualWebRTC.createOffer");
            this.StateHasChanged();
        }

        public async Task AcceptOffer()
        {
            if (string.IsNullOrWhiteSpace(this.RemoteSdp))
            {
                return;
            }

            this.Messages.Add(new ChatMessage("Verarbeite Offer...", "System", UserType.Peer));
            this.LocalSdp = await this.JsManual.InvokeAsync<string>("manualWebRTC.receiveOfferAndCreateAnswer", this.RemoteSdp);
            this.StateHasChanged();
        }

        public async Task AcceptAnswer()
        {
            if (string.IsNullOrWhiteSpace(this.RemoteSdp))
            {
                return;
            }

            bool ok = await this.JsManual.InvokeAsync<bool>("manualWebRTC.receiveAnswer", this.RemoteSdp);
            if (!ok)
            {
                this.Messages.Add(new ChatMessage("Fehler beim Verbinden.", "System", UserType.Peer));
            }

            this.StateHasChanged();
        }

        public async Task OnSend()
        {
            if (string.IsNullOrWhiteSpace(this.Message))
            {
                return;
            }

            bool sent = await this.JsManual.InvokeAsync<bool>("manualWebRTC.sendData", this.Message);

            if (sent)
            {
                this.Messages.Add(new ChatMessage(this.Message, "Me", UserType.Me));
            }
            else
            {
                this.Messages.Add(new ChatMessage("Senden fehlgeschlagen (nicht verbunden).", "System", UserType.Me));
            }

            this.Message = string.Empty;
        }

        public async Task SendOnKeyDown(KeyboardEventArgs pressedKey)
        {
            if (pressedKey.Key == "Enter" || pressedKey.Code == "Enter")
            {
                await this.OnSend();
            }
        }

        [JSInvokable]
        public void ReceiveMessage(string messageText)
        {
            ChatMessage msgObj = new(messageText, "Peer", UserType.Peer);

            msgObj.UpdateUserTypeAndSender();

            this.Messages.Add(msgObj);
            this.StateHasChanged();
        }

        [JSInvokable]
        public void DataChannelStateChanged(string state)
        {
            this.DataChannelState = state;
            this.StateHasChanged();
        }

        public async Task Close()
        {
            await this.JsManual.InvokeVoidAsync("manualWebRTC.close");
            this.DataChannelState = "closed";
            this.Messages.Clear();
            this.StateHasChanged();
        }

        public void Dispose()
        {
            try {
                _ = this.JsManual.InvokeVoidAsync("manualWebRTC.close");
            } catch { }

            this.objRef?.Dispose();
        }
    }
}