﻿using Akka.Actor;
using Akka.Event;
using Asteroids.Shared;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API.Actors;

public class LobbyActor : ReceiveActor
{
    private IActorRef? mapActor;
    private Lobby lobby;
    private readonly ILoggingAdapter Log = Context.GetLogger();

    public LobbyActor(Guid lobbyId)
    {
        lobby = new Lobby { LobbyId = lobbyId };

        Receive<JoinLobbyMessage>(HandleLobbyJoin);
        Receive<LobbyStateChangeMessage>(ChangeStateHandler);
        Receive<LobbyStateMessage>(GetState);
    }

    private void HandleLobbyJoin(JoinLobbyMessage joinLobby)
    {

        if(joinLobby.lobbyId != lobby.LobbyId)
        {
            string errorMsg = "Error invalid lobby ID";
            Log.Error(errorMsg);
            Sender.Tell(new LobbyErrorResponse(errorMsg));
        }

        if (joinLobby.player != null)
        {
            if(lobby.State != LobbyState.JOINING)
            {
                string errorMsg = $"Lobby {lobby.LobbyId} can't add new player {joinLobby.player} because state is not joining";
                Log.Error(errorMsg);
                Sender.Tell(new LobbyErrorResponse(errorMsg));
            }
            lobby.Players.Add(joinLobby.player);
            Log.Info($"Player {joinLobby.player.Username} added to lobby {joinLobby.lobbyId}");
            Sender.Tell(new JoinLobbyResponse(lobby.LobbyId, lobby.Players));
        }
        else
        {
            string errorMsg = "Player not found";
            Log.Error(errorMsg);
            Sender.Tell(new LobbyErrorResponse(errorMsg));
        }
    }

    private void ChangeStateHandler(LobbyStateChangeMessage newState)
    {
        if (newState.lobbyId != lobby.LobbyId)
        {
            string errorMsg = "Error invalid lobby ID";
            Log.Error(errorMsg);
            Sender.Tell(new LobbyErrorResponse(errorMsg));
        }

        switch (newState.state) 
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
        Sender.Tell(new LobbyStateResponse(lobby.LobbyId, lobby.State));
    }

    private void SendPlayersToMap()
    {
        //foreach (var player in lobby.Players)
        //{
        //    mapActor.Tell(new PlayerJoined(player));
        //}
    }

    private void GetState(LobbyStateMessage state)
    {
        if (state.lobbyId != lobby.LobbyId)
        {
            string errorMsg = "Error invalid lobby ID";
            Log.Error(errorMsg);
            Sender.Tell(new LobbyErrorResponse(errorMsg));
        }

        Sender.Tell(new LobbyStateResponse(lobby.LobbyId, lobby.State));
    }
    public static Props Props(Guid lobbyId) => Akka.Actor.Props.Create(() => new LobbyActor(lobbyId));
}
