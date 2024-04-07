using Akka.Actor;
using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR;
using static Asteroids.API.Records;

namespace Asteroids.API.Actors;

public class SignalRActor : ReceiveActor
{
    private readonly SignalRHandler signalRHandler;
    private readonly Dictionary<Type, IActorRef> actorMappings;

    public SignalRActor(SignalRHandler signalRHandler, Dictionary<Type, IActorRef> actorMappings)
    {
        this.signalRHandler = signalRHandler;
        this.actorMappings = actorMappings;

        Receive<JoinLobby>(ForwardJoinLobbyMessage);
        Receive<ChangeLobbyState>(ForwardChangeLobbyStateMessage);
        Receive<GetLobbyState>(message => ForwardLobbyStateMessage(message));
    }

    private void ForwardJoinLobbyMessage(JoinLobby message)
    {
        var lobbyActor = Context.ActorSelection("/user/LobbyActor");
        lobbyActor.Tell(message);
    }

    private void ForwardChangeLobbyStateMessage(ChangeLobbyState message)
    {
        var lobbyActor = Context.ActorSelection("/user/LobbyActor");
        lobbyActor.Tell(message);
    }

    private async Task ForwardLobbyStateMessage(GetLobbyState message)
    {
        var lobbyactor = Context.ActorSelection("/user/LobbyActor");
        (Guid, string) lobbyState = await lobbyactor.Ask<(Guid, string)>(message);

        await signalRHandler.SendLobbyState(lobbyState.Item1, lobbyState.Item2);
    }
}
