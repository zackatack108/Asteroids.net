using Akka.Actor;
using Akka.Event;
using Asteroids.Shared;
using static Asteroids.API.Messages.LobbyMessages;
using static Asteroids.API.Messages.LobbySupervisorMessages;

namespace Asteroids.API.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly ILoggingAdapter log = Context.GetLogger();
    private List<IActorRef> lobbies;

    public LobbySupervisorActor()
    {
        lobbies = new();

        Receive<LobbyCreateMessage>(_ => HandleCreatingLobby(new Guid()));
        Receive<LobbyListMessage>(_ => HandleListLobbyMessage());
        Receive<LobbyTerminateMessage>(HandleTerminateLobbyMessage);

        Receive<Terminated>(HandleLobbyTerminated);
    }

    private void HandleCreatingLobby(Guid lobbyId)
    {
        IActorRef lobby = Context.ActorOf(Akka.Actor.Props.Create(() => new LobbyActor(lobbyId)), lobbyId.ToString());
        Context.Watch(lobby);        
        lobbies.Add(lobby);
    }

    private void HandleListLobbyMessage()
    {
        Task.Run(async () =>
        {
            await ReceiveListLobbyMessageAsync();
        });
    }

    private async Task ReceiveListLobbyMessageAsync()
    {
        var lobbyList = new List<Lobby>();

        foreach (var lobbyActorRef in lobbies)
        {
            var lobbyIdString = lobbyActorRef.Path.Name;

            if (Guid.TryParse(lobbyIdString, out Guid lobbyId))
            {
                var lobbyInfo = await lobbyActorRef.Ask<LobbyInfoResponse>(new LobbyInfoMessage(lobbyId));
                lobbyList.Add(lobbyInfo.lobby);
            }
            else
            {
                log.Error($"Invalid lobby ID format: {lobbyIdString}");
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
            log.Info($"Lobby {message.lobbyId} terminated and removed from supervisor.");
        }
        else
        {
            log.Warning($"Lobby {message.lobbyId} not found.");
        }
    }

    private void HandleLobbyTerminated(Terminated terminated)
    {
        lobbies.Remove(terminated.ActorRef);
        log.Info($"Lobby {terminated.ActorRef.Path.Name} terminated.");

        Guid lobbyId;
        if (Guid.TryParse(terminated.ActorRef.Path.Name, out lobbyId))
        {
            HandleCreatingLobby(lobbyId);
            log.Info($"Restarting lobby {terminated.ActorRef.Path.Name}...");
        }
        else
        {
            log.Error($"Failed to parse lobby ID from actor name: {terminated.ActorRef.Path.Name}");
        }
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new LobbySupervisorActor());
}
