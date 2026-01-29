using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit.Abstractions;

namespace ChatTool.Core.Test;

public sealed class Simulation(ITestOutputHelper output)
{
    private const string HubUrl = "https://chattoolserver-hggqbnchfsd7gxb5.switzerlandnorth-01.azurewebsites.net/messagehub";

    [Fact]
    public async Task JoinRoom_Simulation()
    {
        int[] clients = [1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 500]; // Anzahl Clients per wave

        string roomCode;
        await using (HubConnection host = CreateConnection())
        {
            await host.StartAsync();
            roomCode = await host.InvokeAsync<string>("CreateRoom");
        }

        output.WriteLine("Clients;Success;Failed;Errors;TotalMs;AvgMsPerClient");

        foreach (int clientCount in clients)
        {
            JoinWaveResult result = await this.RunJoinWave(roomCode, clientCount);

            output.WriteLine(
                $"{result.Clients};{result.Success};{result.Failed};{result.Errors};{result.TotalMs};{result.AvgMsPerClient:F2}"
            );
        }
    }

    private async Task<JoinWaveResult> RunJoinWave(string roomCode, int clientCount)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Task<(HubConnection Connection, bool Joined, string? Error)>[] tasks = Enumerable.Range(0, clientCount).Select(async _ =>
        {
            HubConnection connection = CreateConnection();
            try
            {
                await connection.StartAsync();
                bool joined = await connection.InvokeAsync<bool>("JoinRoom", roomCode);
                return (Connection: connection, Joined: joined, Error: null);
            }
            catch (Exception ex)
            {
                return (Connection: connection, Joined: false, Error: ex.Message);
            }
        }).ToArray();

        (HubConnection Connection, bool Joined, string? Error)[] results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        foreach ((HubConnection Connection, bool Joined, string? Error) r in results)
        {
            try { await r.Connection.DisposeAsync(); }
            catch
            {
                // ignored
            }
        }

        int success = results.Count(r => r.Joined);
        int failed = results.Length - success;
        int errors = results.Count(r => r.Error != null);

        return new JoinWaveResult(
            Clients: clientCount,
            Success: success,
            Failed: failed,
            Errors: errors,
            TotalMs: stopwatch.ElapsedMilliseconds
        );
    }

    private static HubConnection CreateConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    private sealed record JoinWaveResult(int Clients, int Success, int Failed, int Errors, long TotalMs)
    {
        public double AvgMsPerClient
        {
            get { return this.Clients == 0 ? 0 : (double)this.TotalMs / this.Clients; }
        }
    }
}