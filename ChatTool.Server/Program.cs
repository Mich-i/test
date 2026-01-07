using ChatTool.Core.Domain.Models;
using ChatTool.Server.Components;
using ChatTool.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

namespace ChatTool.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSignalR();

            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    ["application/octet-stream"]);
            });

            // Server-seitiger "globaler" Zustand
            builder.Services.AddSingleton<UserInformation>();

            WebApplication app = builder.Build();

            app.UseResponseCompression();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAntiforgery();

            // SignalR Hub
            app.MapHub<MessageHub>("/messageHub");

            app.MapStaticAssets();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
