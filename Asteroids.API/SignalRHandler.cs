using Akka.Actor;
using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API;

public class SignalRHandler
{
    private readonly IActorRef signalRActor;
    private HubConnection? hubConnection;
    private readonly string hubUrl;
    public SignalRHandler(IActorRef signalRActor, string hubUrl)
    {
        this.hubUrl = hubUrl;
        this.signalRActor = signalRActor;
        _ = OpenConnectionAsync();
    }

    private async Task OpenConnectionAsync()
    {
        hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).Build();

        hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5));
            await hubConnection.StartAsync();
        };

        await hubConnection.StartAsync();
    }

    private async Task CloseConnectionAsync()
    {
        if (hubConnection != null)
        {
            await hubConnection.StopAsync();
        }
    }
    public void JoinLobby(Guid lobbyId, string username)
    {
        Player player = new Player { Username = username, Bank = 0, Score = 0, Ship = null };
        var joinLobbyMessage = new LobbyJoinMessage(lobbyId, player);
        signalRActor.Tell(joinLobbyMessage);
    }

    public void ChangeLobbyState(Guid lobbyId, string state)
    {
        if(Enum.TryParse(state, true, out LobbyState newState))
        {
            var changeStateMessage = new LobbyChangeStateMessage(lobbyId, newState);
            signalRActor.Tell(changeStateMessage);
        }
    }

    public void GetLobbyState(Guid lobbyId)
    {
        //var getStateMessage = new GetLobbyState(lobby.se);
        //signalRActor.Tell(getStateMessage);
    }

    public async Task SendLobbyState(Guid lobbyId, string state)
    {
        try
        {
            //logger.LogInformation($"Hub Connection State: {hubConnection.State}");

            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                //logger.LogInformation("Connecting to hub");
                await OpenConnectionAsync();
            }
            //logger.LogInformation($"Hub Connection State: {hubConnection.State}");
            if (hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("LobbyState", lobbyId, state);
            }
        }
        catch (Exception ex)
        {
            //logger.LogError(ex.Message);
        }
        
    }
}
