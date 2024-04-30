using Akka.Actor;
using Akka.Event;
using Asteroids.API.Services;
using Asteroids.API.Utils;
using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API.Actors;

public class LobbyActor : ReceiveActor, IWithTimers
{
    private IActorRef supervisor;
    private Lobby lobby;
    private MapUtil mapUtil;
    private readonly ILoggingAdapter Log = Context.GetLogger();
    private readonly SignalRService signalRService;
    public ITimerScheduler Timers { get; set; }

    private const double tickInterval = 0.1;
    private const string runTickTimerKey = "TickTimer";

    public LobbyActor(Guid lobbyId, bool restarted, IActorRef supervisor, SignalRService signalRService)
    {
        if(supervisor == null)
        {
            this.supervisor = this.Self;
        } 
        else
        {
            this.supervisor = supervisor;
        }
        lobby = new Lobby { LobbyId = lobbyId, Map = new() };
        mapUtil = new(lobby.Map, this.Self, supervisor, signalRService);
        this.signalRService = signalRService;

        Receive<LobbyJoinMessage>(HandleLobbyJoin);
        Receive<LobbyChangeStateMessage>(ChangeStateHandler);
        Receive<LobbyCurrentStateMessage>(GetState);
        Receive<LobbyInfoMessage>(GetInfo);
        Receive<LobbyMovePlayerMessage>(MovePlayer);
        Receive<LobbyGetMapMessage>(_ => HandleGettingMap());
        Receive<LobbySetMapSizeMessage>(SetMapSize);
        Receive<LobbyUpdateMapMessage>(_ => UpdateMap());

        if (restarted)
        {

        }
    }

    private void StartTickTimer()
    {
        Timers.StartPeriodicTimer(
            runTickTimerKey,
            new LobbyUpdateMapMessage(),
            TimeSpan.FromSeconds(0.05),
            TimeSpan.FromSeconds(tickInterval)
        );
    }

    private void HandleLobbyJoin(LobbyJoinMessage joinLobby)
    {

        if (joinLobby.username != null)
        {
            if(lobby.State != LobbyState.JOINING)
            {
                string errorMsg = $"Lobby {lobby.LobbyId} can't add new player {joinLobby.username} because state is not joining";
                Log.Error(errorMsg);
                Sender.Tell(new LobbyErrorResponse(errorMsg));
            }
            Player player = new Player { Username = joinLobby.username, Bank = 0, Score = 0, Ship = new() };
            lobby.Map.Players.Add(player);
            Log.Info($"Player {player.Username} added to lobby {lobby.LobbyId}");
            signalRService.GetHub().SendAsync("LobbyInfoResponse", lobby);
            Sender.Tell(new LobbyJoinResponse(lobby));
        }
        else
        {
            string errorMsg = "Player not found";
            Log.Error(errorMsg);
            Sender.Tell(new LobbyErrorResponse(errorMsg));
        }
    }

    private void ChangeStateHandler(LobbyChangeStateMessage newState)
    {
        switch (newState.state) 
        {
            case LobbyState.JOINING:
                lobby.State = newState.state;
                break;
            case LobbyState.ACTIVE:
                lobby.State = newState.state;
                mapUtil.ShufflePlayerLocation();
                StartTickTimer();
                break;
            case LobbyState.RESETTING:
                lobby.State = newState.state;
                lobby = new Lobby { LobbyId = lobby.LobbyId};
                break;
            case LobbyState.INACTIVE:
                lobby.State = newState.state;
                Context.Stop(Self);
                break;
            default:
                Log.Warning($"Unsupported lobby state: {newState.state}");
                break;
        }
        signalRService.GetHub().SendAsync("LobbyStateResponse", lobby.LobbyId, lobby.State);
        Sender.Tell(new LobbyStateResponse(lobby.LobbyId, lobby.State));
    }

    private void GetState(LobbyCurrentStateMessage state)
    {
        signalRService.GetHub().SendAsync("LobbyStateResponse", lobby.LobbyId, lobby.State);
        Sender.Tell(new LobbyStateResponse(lobby.LobbyId, lobby.State));
    }
    
    private void GetInfo(LobbyInfoMessage info)
    {
        signalRService.GetHub().SendAsync("LobbyInfoResponse", lobby);
        Sender.Tell(new LobbyInfoResponse(lobby));
    }

    private void MovePlayer(LobbyMovePlayerMessage message)
    {
        mapUtil.HandleShipMovement(message);
    }

    private void HandleGettingMap()
    {
        Map response = mapUtil.GetMap();
        signalRService.GetHub().SendAsync("MapInfoResponse", response);
        Sender.Tell(new LobbyMapResponse(response));
    }

    private void SetMapSize(LobbySetMapSizeMessage message)
    {
        if (lobby.State.Equals(LobbyState.ACTIVE))
        {
            Sender.Tell(new LobbyErrorResponse("Can't modify size cause map is active"));
            return;
        }
        mapUtil.ChangeMapSize(message.height, message.width);
    }

    private void UpdateMap()
    {
        if (lobby.State != LobbyState.ACTIVE)
        {
            Log.Warning("Cannot update map because lobby is not in the active state.");
            return;
        }

        mapUtil.RunGameTick();
    }

    public static Props Props(Guid lobbyId, bool restarted, IActorRef supervisor, SignalRService signalRService) => Akka.Actor.Props.Create(() => new LobbyActor(lobbyId, restarted, supervisor, signalRService));
}
