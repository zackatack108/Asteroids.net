using Akka.Actor;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API.Actors;

public class SignalRActor : ReceiveActor
{
    private readonly SignalRHandler signalRHandler;
    private readonly Dictionary<Type, IActorRef> actorMappings;

    public SignalRActor(SignalRHandler signalRHandler, Dictionary<Type, IActorRef> actorMappings)
    {
        this.signalRHandler = signalRHandler;
        this.actorMappings = actorMappings;

        Receive<JoinLobbyMessage>(ForwardJoinLobbyMessage);
        Receive<LobbyStateChangeMessage>(ForwardChangeLobbyStateMessage);
        Receive<LobbyStateMessage>(message => ForwardLobbyStateMessage(message));
    }

    private void ForwardJoinLobbyMessage(JoinLobbyMessage message)
    {
        var lobbyActor = Context.ActorSelection("/user/LobbyActor");
        lobbyActor.Tell(message);
    }

    private void ForwardChangeLobbyStateMessage(LobbyStateChangeMessage message)
    {
        var lobbyActor = Context.ActorSelection("/user/LobbyActor");
        lobbyActor.Tell(message);
    }

    private async Task ForwardLobbyStateMessage(LobbyStateMessage message)
    {
        var lobbyactor = Context.ActorSelection("/user/LobbyActor");
        (Guid, string) lobbyState = await lobbyactor.Ask<(Guid, string)>(message);

        await signalRHandler.SendLobbyState(lobbyState.Item1, lobbyState.Item2);
    }
}
