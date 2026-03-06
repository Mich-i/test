using Microsoft.Extensions.Configuration;

namespace ChatTool.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddScoped(sp => new HttpClient());

        Dictionary<string, string> mySettings = new()
        {
            {"Signaling:HubUrl", "https://chattool-server-frh0dqdsbjewf6b4.switzerlandnorth-01.azurewebsites.net/messagehub"},
        };

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(mySettings!);

        builder.Configuration.AddConfiguration(configBuilder.Build());

        builder.Services.AddScoped<Services.SignalingService>();

        return builder.Build();
    }
}
