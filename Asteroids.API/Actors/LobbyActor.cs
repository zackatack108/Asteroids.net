using Akka.Actor;
using Akka.Event;
using Asteroids.Shared;
using static Asteroids.API.Records;

namespace Asteroids.API.Actors;

public class LobbyActor : ReceiveActor
{
    private IActorRef mapActor = Context.ActorOf(Props.Create<MapActor>());
    private Lobby lobby;
    private readonly ILoggingAdapter Log = Context.GetLogger();

    public LobbyActor()
    {
        lobby = new Lobby();

        Receive<JoinLobby>(HandleLobbyJoin);
        Receive<ChangeLobbyState>(ChangeStateHandler);
        Receive<GetLobbyState>(_ => GetState());
    }

    private void HandleLobbyJoin(JoinLobby joinLobby)
    {
        if (joinLobby.player != null)
        {
            if(lobby.State != LobbyState.JOINING)
            {
                Log.Error($"Lobby {lobby.Session} can't add new player {joinLobby.player} because state is not joining");
            }
            lobby.Players.Add(joinLobby.player);
        }
        else
        {
            Log.Error("Player not found");
        }
    }

    private void ChangeStateHandler(ChangeLobbyState newState)
    {
        switch(newState.state) 
        {
            case LobbyState.JOINING:
                lobby.State = newState.state;
                break;
            case LobbyState.ACTIVE:
                lobby.State = newState.state;
                SendPlayersToMap();
                break;
            case LobbyState.RESETTING:
                lobby.State = newState.state;
                lobby = new();
                break;
            case LobbyState.INACTIVE:
                lobby.State = newState.state;
                Context.Stop(Self);
                break;
            default:
                Log.Warning($"Unsupported lobby state: {newState.state}");
                break;
        }
    }

    private void SendPlayersToMap()
    {
        foreach (var player in lobby.Players)
        {
            mapActor.Tell(new PlayerJoined(player));
        }
    }

    private void GetState()
    {
        Sender.Tell((lobby.Session, lobby.State));
    }
}
