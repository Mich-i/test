'use strict';

/*
  Minimal WebRTC (DataChannel) + Manual SDP exchange (copy/paste)
  - No server
  - Bundled ICE candidates (wait until gathering complete)
  - SDP is returned as base64(JSON) for safe clipboard transfer
*/

let pc = null;
let channel = null;
let dotNetRef = null;

// Toggle: set to [] to try "LAN-only" without STUN.
// Warning: Without STUN some networks will fail.
const iceServers = [
    { urls: "stun:stun.l.google.com:19302" }
];

function ensurePeerConnection() {
    if (pc) return;

    pc = new RTCPeerConnection({ iceServers });

    // If we are the answerer, the offerer created the data channel.
    pc.ondatachannel = (event) => {
        attachDataChannel(event.channel);
    };
}

function attachDataChannel(dataChannel) {
    channel = dataChannel;

    channel.onopen = () => notifyState("open");
    channel.onclose = () => notifyState("closed");
    channel.onerror = () => notifyState("error");

    channel.onmessage = (event) => {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("ReceiveMessage", event.data)
                .catch(() => { });
        }
    };
}

function notifyState(state) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync("DataChannelStateChanged", state)
            .catch(() => { });
    }
}

async function waitForIceComplete() {
    if (!pc) return;
    if (pc.iceGatheringState === "complete") return;

    await new Promise((resolve) => {
        const handler = () => {
            if (pc && pc.iceGatheringState === "complete") {
                pc.removeEventListener("icegatheringstatechange", handler);
                resolve();
            }
        };
        pc.addEventListener("icegatheringstatechange", handler);
        handler(); // handle race
    });
}

function encode(obj) {
    const json = JSON.stringify(obj);
    const bytes = new TextEncoder().encode(json);

    let binary = "";
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

function decode(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);

    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }

    const json = new TextDecoder().decode(bytes);
    return JSON.parse(json);
}

window.webRTCInterop = {
    initialize(dotNetObject) {
        dotNetRef = dotNetObject ?? null;
        ensurePeerConnection();
        return true;
    },

    async createOffer() {
        ensurePeerConnection();

        // Offerer creates the channel proactively
        if (!channel) {
            attachDataChannel(pc.createDataChannel("chat"));
        }

        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await waitForIceComplete();

        return encode(pc.localDescription);
    },

    async receiveOfferAndCreateAnswer(remoteBase64) {
        ensurePeerConnection();

        const offerDesc = decode(remoteBase64);
        await pc.setRemoteDescription(offerDesc);

        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        await waitForIceComplete();

        return encode(pc.localDescription);
    },

    async receiveAnswer(remoteBase64) {
        if (!pc) return false;

        const answerDesc = decode(remoteBase64);
        await pc.setRemoteDescription(answerDesc);
        return true;
    },

    sendData(text) {
        if (!channel || channel.readyState !== "open") return false;

        channel.send(text);
        return true;
    },

    isDataChannelOpen() {
        return !!channel && channel.readyState === "open";
    },

    close() {
        try {
            channel?.close();
        } catch (e) { }

        try {
            pc?.close();
        } catch (e) { }

        pc = null;
        channel = null;
        dotNetRef = null;
    }
};