using Microsoft.AspNetCore.SignalR.Client;

namespace Asteroids.API.Services;

public class SignalRService
{
    private readonly HubConnection hubConnection;
    public SignalRService(string hubUrl)
    {
        hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).Build();

        hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5));
            await hubConnection.StartAsync();
        };

        _ = StartConnectionAsync();
    }

    public async Task StartConnectionAsync()
    {
        try
        {
            await hubConnection.StartAsync();
            Console.WriteLine("SignalR connected");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
        }
    }

    public HubConnection GetHub()
    {
        return hubConnection;
    }
}
