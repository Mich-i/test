using ChatTool.Client.Application;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatTool.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<ClientSession>();

            // Register an initialized HubConnection so components can inject it.
            builder.Services.AddSingleton(sp =>
            {
                // Build hub URL relative to the host environment base address
                var hubUri = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "MessageHub");
                var connection = new HubConnectionBuilder()
                    .WithUrl(hubUri)
                    .WithAutomaticReconnect()
                    .Build();

                return connection;
            });

            await builder.Build().RunAsync();
        }
    }
}
