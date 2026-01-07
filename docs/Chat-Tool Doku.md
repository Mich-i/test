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
-Proxies/ Client-Adapter zu SignalR (zB. ServerProxy) (NOCH NICHT IMPLEMENTIERT)
-State/ einfacher UI-State (aktuelle Channels, Messages etc.)(NOCH NICHT IMPLEMENTIERT)
-App.razor / Routes.razor Einstieg & Routing

ChatTool.Server (ASP.NET Core)
-Application/Hubs/ dünner SignalR-Hub (ChatHub) – nur Entgegennahme/Weiterleitung
-Application/Services/ Server-Logik & Zustand (zB: ClientRegistry, ChannelRegistry, MessageDispatcher) (NOCH NICHT IMPLEMENTIERT)
-Application/Proxies/ Server-Adapter für Server→Client-Sends (zB: ClientProxy) (NOCH NICHT IMPLEMENTIERT)

ChatTool.Core (Shared)
-Domain/Models/ DTOs (zB: ClientInfo, ChannelInfo, ChatMessage, ClientId, ChannelId) (NOCH NICHT IMPLEMENTIERT)
-Domain/Abstractions/ Interfaces (ILoginManager, IChannelManager, IMessageManager) (NOCH NICHT IMPLEMENTIERT)
-(Keinerlei UI- oder Framework-Abhängigkeiten)


Rollen/Verantwortung:

-Client: UI rendern, lokale Ansicht halten, über ServerProxy aufrufen

-Server: Source of Truth für Online-User (Channels); verteilt Nachrichten an Channel-Mitglieder.

-Core: Gemeinsame Sprache (Modelle + Interfaces) für Client und Server.

-SignalR: Transport (Events), keine Geschäftslogik.