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

Projekte:

ChatTool.Client (Blazor WASM)
-Components/ UI (Layout, Nav, Seiten)
-Pages/ Chat-Seiten (Home, Public/Private/Group)
-wwwroot/webrtc.js (Logik für Peer-to-Peer Kommunikation)
-App.razor / Routes.razor Einstieg & Routing

ChatTool.Core (Shared)
-(Keinerlei UI- oder Framework-Abhängigkeiten)


Rollen/Verantwortung:

-Client: UI rendern, lokale Ansicht halten, P2P-Verbindung via WebRTC aufbauen.

-Core: Gemeinsame Sprache (Modelle + Interfaces) für Client.

-WebRTC (`webrtc.js`): Stellt die Transportlogik für die direkte Peer-to-Peer-Kommunikation bereit.

---

### WebRTC Logik (`webrtc.js`)

Diese Datei kapselt die minimale WebRTC-Logik für eine direkte Peer-to-Peer-Verbindung über einen `RTCDataChannel`.
Der Verbindungsaufbau erfolgt **manuell per Copy/Paste** (Offer/Answer). Es wird **kein Trickle-ICE** genutzt:
Offer/Answer werden erst zurückgegeben, nachdem das ICE-Gathering abgeschlossen ist, damit die SDP die benötigten
ICE Candidates bereits enthält.

#### Kernobjekte
- **`RTCPeerConnection` (`pc`)**: baut die Verbindung zwischen zwei Peers auf (SDP + ICE).
- **`RTCDataChannel` (`channel`)**: sendet/empfängt Textnachrichten über die PeerConnection.
- **STUN (`iceServers`)**: standardmässig `stun:stun.l.google.com:19302` zur besseren Verbindung über NAT.

#### Ablauf (Signaling)
1. **Offerer** ruft `createOffer()` auf, kopiert die zurückgegebene Base64-SDP und sendet sie manuell an den Partner.
2. **Answerer** ruft `receiveOfferAndCreateAnswer(offerBase64)` auf und sendet die resultierende Base64-SDP zurück.
3. **Offerer** ruft `receiveAnswer(answerBase64)` auf → Verbindung wird abgeschlossen.
4. Nachrichten können mit `sendData(text)` gesendet werden.

#### Blazor-Interop (`window.webRTCInterop`)
| Funktion | Zweck |
|---|---|
| `initialize(dotNetObject)` | Speichert .NET-Referenz und initialisiert die PeerConnection |
| `createOffer()` | Erstellt DataChannel (Offerer), setzt LocalDescription, wartet auf ICE complete, gibt Base64-SDP zurück |
| `receiveOfferAndCreateAnswer(remoteBase64)` | Setzt Remote-Offer, erstellt Answer, wartet auf ICE complete, gibt Base64-SDP zurück |
| `receiveAnswer(remoteBase64)` | Setzt Remote-Answer und finalisiert den Handshake |
| `sendData(text)` | Sendet Text über den offenen DataChannel (nur wenn `readyState === "open"`) |
| `isDataChannelOpen()` | Prüft, ob der DataChannel offen ist |
| `close()` | Schliesst Channel/Connection und setzt interne Referenzen zurück |

#### Callbacks von JavaScript nach .NET
| .NET Methode (`[JSInvokable]`) | Wann |
|---|---|
| `DataChannelStateChanged(state)` | Bei `channel.onopen`, `channel.onclose`, `channel.onerror` (`open/closed/error`) |
| `ReceiveMessage(message)` | Bei eingehender Nachricht über `channel.onmessage` |

#### Encoding
Offer/Answer werden als `base64(JSON)` übertragen:
- `encode(obj)`: JSON → UTF-8 Bytes → Base64
- `decode(base64)`: Base64 → UTF-8 → JSON