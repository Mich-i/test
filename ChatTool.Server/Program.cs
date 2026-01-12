using ChatTool.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev-client", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("dev-client");

app.MapHub<MessageHub>("/messagehub");

app.Run();