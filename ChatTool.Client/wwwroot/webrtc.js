'use strict';

const peers = {}; // Key: connectionId, Value: { pc, channel }
let dotNetRef = null;
const iceServers = [{ urls: "stun:stun.l.google.com:19302" }];

window.webRTCInterop = {
    initialize(dotNetObject) {
        dotNetRef = dotNetObject ?? null;
        return true;
    },

    async createOffer(peerId) {
        const pc = new RTCPeerConnection({ iceServers });
        const channel = pc.createDataChannel("chat");

        peers[peerId] = { pc, channel };
        setupEvents(peerId, pc, channel);

        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await waitForIce(pc);

        return encode(pc.localDescription);
    },

    async receiveOfferAndCreateAnswer(peerId, offerBase64) {
        const pc = new RTCPeerConnection({ iceServers });
        peers[peerId] = { pc };

        pc.ondatachannel = (event) => {
            peers[peerId].channel = event.channel;
            setupEvents(peerId, pc, event.channel);
        };

        const offerDesc = decode(offerBase64);
        await pc.setRemoteDescription(offerDesc);

        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        await waitForIce(pc);

        return encode(pc.localDescription);
    },

    async receiveAnswer(peerId, answerBase64) {
        const peer = peers[peerId];
        if (peer) {
            await peer.pc.setRemoteDescription(decode(answerBase64));
            return true;
        }
        return false;
    },

    sendData(text) {
        let anySent = false;
        for (const id in peers) {
            const channel = peers[id].channel;
            if (channel && channel.readyState === "open") {
                channel.send(text);
                anySent = true;
            }
        }
        return anySent;
    },

    close() {
        for (const id in peers) {
            peers[id].pc.close();
            delete peers[id];
        }
    }
};

function setupEvents(peerId, pc, channel) {
    channel.onopen = () => notifyState(peerId, "open");
    channel.onmessage = (event) => {
        dotNetRef?.invokeMethodAsync("ReceiveMessage", event.data);
    };
}

async function waitForIce(pc) {
    if (pc.iceGatheringState === "complete") return;
    await new Promise(res => {
        const check = () => {
            if (pc.iceGatheringState === "complete") {
                pc.removeEventListener("icegatheringstatechange", check);
                res();
            }
        };
        pc.addEventListener("icegatheringstatechange", check);
    });
}

// Gleich wie vorher einfach in kurzer Form
function encode(obj) { return btoa(JSON.stringify(obj)); }
function decode(base64) { return JSON.parse(atob(base64)); }
function notifyState(id, state) { dotNetRef?.invokeMethodAsync("DataChannelStateChanged", id + ": " + state); }