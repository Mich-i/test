using System.IO;
using ChatTool.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Optional: read PFX password from configuration or environment variable
// Set DEV_CERT_PASSWORD environment variable to the password you used when creating the PFX.
string certPassword = builder.Configuration["DevCert:Password"]
                      ?? Environment.GetEnvironmentVariable("DEV_CERT_PASSWORD")
                      ?? string.Empty;

// Configure Kestrel to use dev certificate when available
var certPath = Path.Combine(builder.Environment.ContentRootPath, "certs", "dev.p12");
if (File.Exists(certPath))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // HTTPS port (adjust to match your launchSettings or preferred port)
        options.ListenAnyIP(7033, listenOptions => listenOptions.UseHttps(certPath, certPassword));
        // Optional HTTP listener for dev (adjust or remove if not needed)
        options.ListenAnyIP(5021);
    });
}

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