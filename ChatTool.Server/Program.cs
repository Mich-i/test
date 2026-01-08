using ChatTool.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR für Signaling
builder.Services.AddSignalR();

// CORS ist nötig, wenn Client (WASM) auf anderem Origin läuft (anderer Port!).
// Trage hier die URLs ein, auf denen dein ChatTool.Client läuft.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:5000",
                "http://localhost:5000",
                "https://localhost:7000",
                "http://localhost:7000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("ClientCors");

// Hub (alles lowercase, damit Client/Server 100% matchen)
app.MapHub<MessageHub>("/messagehub");

app.Run();