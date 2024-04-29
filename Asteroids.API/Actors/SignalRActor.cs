using Akka.Actor;
using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API.Actors;

public class SignalRActor : ReceiveActor
{
    //private readonly Dictionary<Type, IActorRef> actorMappings;
    IActorRef lobbySupervisor;

    private HubConnection? hubConnection;
    private readonly string hubUrl;

    public SignalRActor(IActorRef lobbySupervisor, string hubUrl)
    {
        this.lobbySupervisor = lobbySupervisor;

        _ = OpenConnectionAsync();

        Receive<LobbyJoinMessage>(JoinLobby);
        Receive<LobbyChangeStateMessage>(ChangeLobbyState);
        Receive<LobbyStateResponse>(SendLobbyState);
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

    public void JoinLobby(LobbyJoinMessage message)
    {
        lobbySupervisor.Tell(message);
    }

    public void ChangeLobbyState(LobbyChangeStateMessage message)
    {
        lobbySupervisor.Tell(message);
    }

    public void GetLobbyState(Guid lobbyId)
    {
        //var getStateMessage = new GetLobbyState(lobby.se);
        //signalRActor.Tell(getStateMessage);
    }

    public void SendLobbyState(LobbyStateResponse message)
    {
        try
        {
            //logger.LogInformation($"Hub Connection State: {hubConnection.State}");

            if (hubConnection?.State == HubConnectionState.Disconnected)
            {
                //logger.LogInformation("Connecting to hub");
                _ = OpenConnectionAsync();
            }
            //logger.LogInformation($"Hub Connection State: {hubConnection.State}");
            if (hubConnection?.State == HubConnectionState.Connected)
            {
                hubConnection.SendAsync("LobbyState", message.state);
            }
        }
        catch (Exception ex)
        {
            //logger.LogError(ex.Message);
        }


    }
    public static Props Props(IActorRef lobbySupervisor, string hubUrl)
            => Akka.Actor.Props.Create(() => new SignalRActor(lobbySupervisor, hubUrl));
}
