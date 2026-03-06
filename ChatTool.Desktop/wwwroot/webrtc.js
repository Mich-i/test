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

function encode(obj) { return btoa(JSON.stringify(obj)); }
function decode(base64) { return JSON.parse(atob(base64)); }
function notifyState(id, state) { dotNetRef?.invokeMethodAsync("DataChannelStateChanged", id + ": " + state); }

(function () {
    let manualPc = null;
    let manualDataChannel = null;
    let manualDotNetRef = null;
    let manualSendQueue = [];

    const rtcConfig = {
        iceServers: [{ urls: "stun:stun.l.google.com:19302" }],
        iceTransportPolicy: "all"
    };

    function createManualPeerConnection() {
        if (manualPc) return;
        manualPc = new RTCPeerConnection(rtcConfig);

        manualPc.onicecandidate = (ev) => {
        };

        manualPc.ondatachannel = (ev) => {
            setupManualDataChannel(ev.channel);
        };
    }

    function setupManualDataChannel(channel) {
        manualDataChannel = channel;
        manualDataChannel.onopen = () => {
            flushManualQueue();
            if (manualDotNetRef) manualDotNetRef.invokeMethodAsync("DataChannelStateChanged", "open");
        };
        manualDataChannel.onclose = () => {
            if (manualDotNetRef) manualDotNetRef.invokeMethodAsync("DataChannelStateChanged", "closed");
        };
        manualDataChannel.onmessage = (evt) => {
            if (manualDotNetRef) manualDotNetRef.invokeMethodAsync("ReceiveMessage", evt.data);
        };
    }

    function flushManualQueue() {
        while (manualSendQueue.length > 0 && manualDataChannel && manualDataChannel.readyState === "open") {
            manualDataChannel.send(manualSendQueue.shift());
        }
    }

    function waitForIceGathering(pc) {
        if (pc.iceGatheringState === "complete") return Promise.resolve();
        return new Promise(resolve => {
            const check = () => {
                if (pc.iceGatheringState === "complete") {
                    pc.removeEventListener("icegatheringstatechange", check);
                    resolve();
                }
            };
            pc.addEventListener("icegatheringstatechange", check);
            setTimeout(resolve, 5000);
        });
    }

    function encodeDesc(desc) {
        return btoa(JSON.stringify(desc));
    }
    function decodeDesc(b64) {
        return JSON.parse(atob(b64));
    }

    window.manualWebRTC = {
        initialize: function (dotNetObject) {
            manualDotNetRef = dotNetObject;
            createManualPeerConnection();
        },
        createOffer: async function () {
            createManualPeerConnection();
            const dc = manualPc.createDataChannel("chat");
            setupManualDataChannel(dc);

            const offer = await manualPc.createOffer();
            await manualPc.setLocalDescription(offer);

            await waitForIceGathering(manualPc);

            return encodeDesc(manualPc.localDescription);
        },
        receiveOfferAndCreateAnswer: async function (remoteBase64) {
            createManualPeerConnection();
            await manualPc.setRemoteDescription(decodeDesc(remoteBase64));

            const answer = await manualPc.createAnswer();
            await manualPc.setLocalDescription(answer);

            await waitForIceGathering(manualPc);
            return encodeDesc(manualPc.localDescription);
        },
        receiveAnswer: async function (remoteBase64) {
            if (!manualPc) return false;
            await manualPc.setRemoteDescription(decodeDesc(remoteBase64));
            return true;
        },
        sendData: function (text) {
            if (manualDataChannel && manualDataChannel.readyState === "open") {
                manualDataChannel.send(text);
                return true;
            }
            manualSendQueue.push(text);
            return false;
        },
        isDataChannelOpen: function () {
            return !!(manualDataChannel && manualDataChannel.readyState === "open");
        },
        close: function () {
            if (manualDataChannel) manualDataChannel.close();
            if (manualPc) manualPc.close();
            manualPc = null;
            manualDataChannel = null;
        }
    };
})();