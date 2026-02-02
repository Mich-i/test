# Chat-Tool Dokumentation

**Ausgangslage**
"Benötigt wird ein lokales Tool, mit welchem innerhalb des Teams Kommuniziert werden kann.

Um das Tool OS-Unabhängig zu brauchen, wäre wohl eine Technologie wie Blazor WASM eine gute Lösung.

Das ganze kann vorerst ohne dedizierten Server, im LAN funktionieren, da wir die IP des jeweils anderen kennen, bzw. anfragen können.

In einem Weiteren Schritt, sollten die Instanzen des Tools dann aber auch über das Internet verbunden werden können ohne dass die IP der anderen Teilnehmer bekannt ist.

Die Kommunikation kann sich vorerst auf reinen Text beschränken. Bilder, Anhänge, Live Audio Video (voice call/video call) als Erweiterungen wären natürlich toll."

**Technologie:** Blazor WebAssembly, im LAN

**Entwicklungsumgebung:** Visual Studio (Blazor WebAssembly Standalone App)


**Tutorials:**
- Blazor WebAssembly allgemeine Tutorial Seite [blazorschool.com](https://blazorschool.com/tutorial/blazor-webassembly-standalone/dotnet9/introduction-to-blazor-webassembly-standalone)
- Youtube Playlist von DotNet [youtube.com/DotNet](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oXNZX1Q8rB-5xgTSKR8qA5k)
- Blazor in 100 secs [youtube.com/Fireship](https://www.youtube.com/watch?v=QXxNlpjnulI)
- Microsoft SignalR: https://learn.microsoft.com/de-ch/aspnet/core/blazor/tutorials/signalr-blazor?view=aspnetcore-9.0&utm_source=chatgpt.com&tabs=visual-studio


**Aktuelle Architektur:**

Jeder Client verbindet sich zuerst mit dem Server.
Der Server kennt die Räume und vermittelt die Verbindungen.
Sobald die Peers verbunden sind, läuft der Chat direkt zwischen den Browsern – ohne den Server zu belasten.


Rollen/Verantwortung:

-Client: UI rendern, lokale Ansicht halten, P2P-Verbindung via WebRTC aufbauen.

-Core: Gemeinsame Sprache (Modelle + Interfaces) für Client.

-WebRTC (`webrtc.js`): Stellt die Transportlogik für die direkte Peer-to-Peer-Kommunikation bereit.

---

### WebRTC Logik (`webrtc.js`)

Diese Datei kapselt die minimale WebRTC-Logik für direkte Peer-to-Peer-Verbindungen über `RTCDataChannel`. Der Verbindungsaufbau erfolgt **manuell per Copy/Paste** (Offer/Answer). Es wird **kein Trickle-ICE** genutzt: Offer/Answer werden erst zurückgegeben, nachdem das ICE-Gathering abgeschlossen ist, damit die SDP die benötigten ICE Candidates bereits enthält.

Wichtig: Die aktuelle Implementierung unterstützt mehrere gleichzeitige Peers. Intern wird ein `peers`-Objekt verwendet, das Verbindungen per `peerId` verwaltet.

#### Kernobjekte
- **`RTCPeerConnection` (`pc`)**: baut die Verbindung zwischen zwei Peers auf (SDP + ICE).
- **`RTCDataChannel` (`channel`)**: sendet/empfängt Textnachrichten über die PeerConnection.
- **STUN (`iceServers`)**: standardmässig `stun:stun.l.google.com:19302` zur besseren Verbindung über NAT.

#### Ablauf (Signaling)
1. **Offerer** ruft `createOffer(peerId)` auf, kopiert die zurückgegebene Base64-SDP und sendet sie manuell an den Partner (zugeordnet durch `peerId`).
2. **Answerer** ruft `receiveOfferAndCreateAnswer(peerId, offerBase64)` auf und sendet die resultierende Base64-SDP zurück.
3. **Offerer** ruft `receiveAnswer(peerId, answerBase64)` auf → Verbindung wird abgeschlossen.
4. Nach `channel.onopen` können Nachrichten mit `sendData(text)` gesendet werden (die Implementierung sendet an alle offenen DataChannels).

#### Blazor-Interop (`window.webRTCInterop`)
| Funktion | Zweck |
|---|---|
| `initialize(dotNetObject)` | Speichert .NET-Referenz (`dotNetRef`) für Callbacks |
| `createOffer(peerId)` | Erstellt `RTCPeerConnection` + `RTCDataChannel` (als Offerer), setzt LocalDescription, wartet auf ICE complete, gibt Base64-SDP zurück; Verbindung wird unter `peers[peerId]` gehalten |
| `receiveOfferAndCreateAnswer(peerId, offerBase64)` | Erstellt `RTCPeerConnection`, registriert `ondatachannel`, setzt Remote-Offer, erstellt Answer, wartet auf ICE complete, gibt Base64-SDP zurück; Verbindung wird unter `peers[peerId]` gehalten |
| `receiveAnswer(peerId, answerBase64)` | Setzt Remote-Answer für die Verbindung mit `peerId` und finalisiert den Handshake |
| `sendData(text)` | Sendet Text an alle offenen DataChannels (iteriert über `peers`) |
| `close()` | Schliesst alle PeerConnections und leert das `peers`-Objekt |

Hinweis: Es gibt in der aktuellen Version keine eigenständige `isDataChannelOpen()`-Funktion; Statusmeldungen kommen asynchron per Callback.

#### Callbacks von JavaScript nach .NET
| .NET Methode (`[JSInvokable]`) | Wann / Payload |
|---|---|
| `DataChannelStateChanged(state)` | Wird aufgerufen bei `channel.onopen`, `channel.onclose`, `channel.onerror`. Payload ist ein String im Format `"peerId: state"` (z.B. `"peer1: open"`) |
| `ReceiveMessage(message)` | Bei eingehender Nachricht über `channel.onmessage` — `message` ist der empfangene Text |

#### Encoding
Offer/Answer werden als `base64(JSON)` übertragen:
- `encode(obj)`: `JSON.stringify(obj)` → `btoa(...)` (Base64)
- `decode(base64)`: `atob(...)` → `JSON.parse(...)`

Die Implementierung verwendet `btoa`/`atob` auf JSON-Text; das ist die aktuelle, einfache Kodierung in `webrtc.js`