using ChatTool.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

string certPassword = builder.Configuration["DevCert:Password"]
                      ?? Environment.GetEnvironmentVariable("DEV_CERT_PASSWORD")
                      ?? string.Empty;

string certPath = Path.Combine(builder.Environment.ContentRootPath, "certs", "dev.p12");
if (File.Exists(certPath))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(7033, listenOptions => listenOptions.UseHttps(certPath, certPassword));
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