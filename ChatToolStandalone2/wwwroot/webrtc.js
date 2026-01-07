// Manual (copy/paste) signaling WebRTC helper - data-channel only (no server)
// Returns/accepts base64-encoded JSON SDP for clipboard exchange.

'use strict';

let pc = null;
let dataChannel = null;
let dotNetRef = null;
let sendQueue = [];

function createPeerConnection() {
    if (pc) return;
    const config = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] }; // Public STUN Server von Google, um mit Nat hinter einer Firewall die eigene öffentliche IP und Port zu ermitteln
    pc = new RTCPeerConnection(config);

    pc.onicecandidate = (ev) => {
        console.debug("onicecandidate:", ev.candidate);
    };

    pc.ondatachannel = (ev) => {
        console.debug("Remote data channel received");
        setupDataChannel(ev.channel);
    };

    pc.onconnectionstatechange = () => {
        console.debug("Connection state:", pc.connectionState);
    };
}

function setupDataChannel(channel) {
    dataChannel = channel;

    dataChannel.onopen = () => {
        console.debug("Data channel open");
        flushSendQueue();
        if (dotNetRef) dotNetRef.invokeMethodAsync("DataChannelStateChanged", "open").catch((e) => { console.warn("invokeMethodAsync failed:", e); });
    };
    dataChannel.onclose = () => {
        console.debug("Data channel closed");
        if (dotNetRef) dotNetRef.invokeMethodAsync("DataChannelStateChanged", "closed").catch((e) => { console.warn("invokeMethodAsync failed:", e); });
    };
    dataChannel.onerror = (e) => {
        console.error("Data channel error", e);
        if (dotNetRef) dotNetRef.invokeMethodAsync("DataChannelStateChanged", "error").catch((err) => { console.warn("invokeMethodAsync failed:", err); });
    };
    dataChannel.onmessage = (evt) => {
        console.debug("Received:", evt.data);
        if (dotNetRef) dotNetRef.invokeMethodAsync("ReceiveMessage", evt.data).catch((e) => { console.warn("invokeMethodAsync failed:", e); });
    };
}

function flushSendQueue() {
    if (!dataChannel) return;
    while (sendQueue.length > 0 && dataChannel.readyState === "open") {
        const msg = sendQueue.shift();
        try {
            dataChannel.send(msg);
        } catch (e) {
            console.warn("Failed to send queued message", e);
            sendQueue.unshift(msg);
            break;
        }
    }
}

function waitForIceGatheringComplete(pcRef) {
    if (!pcRef) return Promise.resolve();
    if (pcRef.iceGatheringState === "complete") return Promise.resolve();
    return new Promise(resolve => {
        function check() {
            if (pcRef.iceGatheringState === "complete") {
                pcRef.removeEventListener("icegatheringstatechange", check);
                resolve();
            }
        }
        pcRef.addEventListener("icegatheringstatechange", check);
    });
}

function encodeDesc(obj) {
    const json = JSON.stringify(obj);
    const encoder = new TextEncoder();
    const bytes = encoder.encode(json);
    let binary = "";
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

function decodeDesc(b64) {
    const binary = atob(b64);
    const len = binary.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    const decoder = new TextDecoder();
    const json = decoder.decode(bytes);
    return JSON.parse(json);
}

window.clipboard = {
    readText: async function () {
        try { return await navigator.clipboard.readText(); }
        catch (e) { console.warn("clipboard.readText failed", e); return ""; }
    },
    writeText: async function (text) {
        try { await navigator.clipboard.writeText(text); return true; }
        catch (e) { console.warn("clipboard.writeText failed", e); return false; }
    }
};

window.webRTCInterop = {
    initialize: function (dotNetObject) {
        dotNetRef = dotNetObject || null;
        createPeerConnection();
        return true;
    },

    createOffer: async function () {
        createPeerConnection();
        try { const dc = pc.createDataChannel("chat"); setupDataChannel(dc); } catch (e) { console.warn("createDataChannel failed:", e); }
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await waitForIceGatheringComplete(pc);
        return encodeDesc(pc.localDescription);
    },

    receiveOfferAndCreateAnswer: async function (remoteBase64) {
        createPeerConnection();
        try {
            const remoteDesc = decodeDesc(remoteBase64);
            await pc.setRemoteDescription(remoteDesc);
        } catch (e) {
            console.error("Invalid remote offer base64", e);
            return null;
        }
        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        await waitForIceGatheringComplete(pc);
        return encodeDesc(pc.localDescription);
    },

    receiveAnswer: async function (remoteBase64) {
        if (!pc) { console.error("No RTCPeerConnection when receiving answer"); return false; }
        try {
            const remoteDesc = decodeDesc(remoteBase64);
            await pc.setRemoteDescription(remoteDesc);
            return true;
        } catch (e) {
            console.error("Invalid remote answer base64", e);
            return false;
        }
    },

    sendData: function (text) {
        if (dataChannel && dataChannel.readyState === "open") {
            try { dataChannel.send(text); return true; }
            catch (e) { console.warn("send failed, queueing", e); sendQueue.push(text); return false; }
        }
        sendQueue.push(text);
        return false;
    },

    isDataChannelOpen: function () {
        return !!(dataChannel && dataChannel.readyState === "open");
    },

    close: function () {
        try { if (dataChannel) dataChannel.close(); } catch (e) { console.warn("close dataChannel failed", e); }
        try { if (pc) { pc.close(); pc = null; } } catch (e) { console.warn("close pc failed", e); }
        dotNetRef = null;
        dataChannel = null;
        sendQueue = [];
    }
};