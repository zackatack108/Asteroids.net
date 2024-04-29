using Akka.Actor;
using Akka.Event;
using Asteroids.Shared;
using static Asteroids.API.Messages.LobbyMessages;
using static Asteroids.API.Messages.LobbySupervisorMessages;

namespace Asteroids.API.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private List<IActorRef> lobbies;
    private List<IActorRef> deactivatedLobbies;
    protected ILoggingAdapter Log { get; } = Context.GetLogger();

    public LobbySupervisorActor()
    {
        lobbies = new();
        deactivatedLobbies = new();

        Receive<LobbyCreateMessage>(_ => HandleCreatingLobby(Guid.NewGuid(), false));
        Receive<LobbyListMessage>(_ => HandleListLobbyMessage());
        Receive<LobbyTerminateMessage>(HandleTerminateLobbyMessage);
        Receive<LobbyCrashMessage>(HandleLobbyCrash);
        Receive<LobbyJoinMessage>(ForwardJoinMessage);
        Receive<LobbyChangeStateMessage>(ForwardChangeStateMessage);
        Receive<LobbyCurrentStateMessage>(ForwardCurrentStateMessage);
        Receive<LobbyInfoMessage>(ForwardLobbyInfoMessage);
        Receive<LobbyMovePlayerMessage>(ForwardMovePlayerMessage);
        Receive<LobbyGetMapMessage>(ForwardGetMapMessage);
        Receive<LobbySetMapSizeMessage>(ForwardMapSizeMessage);

        Receive<Terminated>(HandleLobbyTerminated);
    }

    private void HandleLobbyCrash(LobbyCrashMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if(lobbyRef != null)
        {
            Context.Stop(lobbyRef);
            Log.Info($"Lobby {lobbyId} has been terminated");
        }
        else
        {
            Log.Warning($"Lobby {message.lobbyId} not found.");
        }
    }

    private void HandleCreatingLobby(Guid lobbyId, bool restarted)
    {
        IActorRef lobby = Context.ActorOf(Akka.Actor.Props.Create(() => new LobbyActor(lobbyId, restarted, this.Self)), lobbyId.ToString());
        Context.Watch(lobby);        
        lobbies.Add(lobby);
        HandleListLobbyMessage();
    }

    private void HandleListLobbyMessage()
    {
        var lobbyList = new List<Lobby>();

        foreach (var lobbyActorRef in lobbies)
        {
            var lobbyIdString = lobbyActorRef.Path.Name;

            if (Guid.TryParse(lobbyIdString, out Guid lobbyId))
            {
                var lobbyInfoResponse = lobbyActorRef.Ask<LobbyInfoResponse>(new LobbyInfoMessage(lobbyId));
                if(lobbyInfoResponse != null)
                {
                    lobbyList.Add(lobbyInfoResponse.Result.lobby);
                }
            }
            else
            {
                Log.Error($"Invalid lobby ID format: {lobbyIdString}");
            }
        }

        Sender.Tell(new LobbyListResponse(lobbyList));
    }    

    private void HandleTerminateLobbyMessage(LobbyTerminateMessage message)
    {
        var lobbyToRemove = lobbies.Find(actor => actor.Path.Name == message.lobbyId.ToString());
        if (lobbyToRemove != null)
        {
            Context.Stop(lobbyToRemove);
            lobbies.Remove(lobbyToRemove);
            Log.Info($"Lobby {message.lobbyId} terminated and removed from supervisor.");
        }
        else
        {
            Log.Warning($"Lobby {message.lobbyId} not found.");
        }
    }

    private void HandleLobbyTerminated(Terminated terminated)
    {
        lobbies.Remove(terminated.ActorRef);
        Log.Info($"Lobby {terminated.ActorRef.Path.Name} terminated.");

        Guid lobbyId;
        if (Guid.TryParse(terminated.ActorRef.Path.Name, out lobbyId))
        {
            var deactivated = deactivatedLobbies.FirstOrDefault(x => x.Path.Name == terminated.ActorRef.Path.Name);
            if (deactivated == default)
            {
                HandleCreatingLobby(lobbyId, true);
                Log.Info($"Restarting lobby {terminated.ActorRef.Path.Name}...");
            }
            else
            {
                deactivatedLobbies.Remove(deactivated);
            }

        }
        else
        {
            Log.Error($"Failed to parse lobby ID from actor name: {terminated.ActorRef.Path.Name}");
        }
    }

    private void ForwardJoinMessage(LobbyJoinMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if(lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardMapSizeMessage(LobbySetMapSizeMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardGetMapMessage(LobbyGetMapMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardMovePlayerMessage(LobbyMovePlayerMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardLobbyInfoMessage(LobbyInfoMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardCurrentStateMessage(LobbyCurrentStateMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    private void ForwardChangeStateMessage(LobbyChangeStateMessage message)
    {
        string lobbyId = message.lobbyId.ToString();

        var lobbyRef = lobbies.FirstOrDefault(x => x.Path.Name == lobbyId);

        if (lobbyRef != null)
        {
            if(message.state == LobbyState.INACTIVE)
            {
                deactivatedLobbies.Add(lobbyRef);
            }
            var result = lobbyRef.Ask(message);
            result.PipeTo(Sender);
        }
        else
        {
            Log.Error($"Couldn't find lobby with id {lobbyId}");
        }
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new LobbySupervisorActor());
}
