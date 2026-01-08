using ChatTool.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin != null &&
                (origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
                 origin.StartsWith("http://192.168.20.212:", StringComparison.OrdinalIgnoreCase)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("client");

app.MapHub<MessageHub>("/messagehub");

app.Run();