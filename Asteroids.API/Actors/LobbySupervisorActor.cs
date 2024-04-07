using Akka.Actor;
using Akka.Event;
using Asteroids.Shared;
using static Asteroids.API.Records;

namespace Asteroids.API.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly ILoggingAdapter log = Context.GetLogger();

    public LobbySupervisorActor()
    {
        Receive<CreateLobby>(_ => CreateLobby());
    }

    private void CreateLobby()
    {
        var childLobbies = Context.GetChildren();
        var allLobbiesNotJoining = childLobbies.All(actor =>
        {
            var lobbyActor = actor as ICanTell;
            var lobbyStateTask = lobbyActor.Ask<LobbyState>(new GetLobbyState());
            return lobbyStateTask.Result != LobbyState.JOINING;
        });

        if (allLobbiesNotJoining)
        {
            var lobbyActor = Context.ActorOf(Akka.Actor.Props.Create<LobbyActor>(), "lobby-" + Context.GetChildren().Count());
            log.Info($"Lobby {lobbyActor.Path.Name} created.");
        }
        else
        {
            log.Info("Not all exising lobbies are in the JOINING state. Skipping lobby creation");
        }
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 10,
            withinTimeMilliseconds: 5000,
            decider: Decider.From(x =>
            {
                switch (x)
                {
                    case ArithmeticException _:
                        return Directive.Resume;
                    case NotSupportedException _:
                        return Directive.Restart;
                    default:
                        return Directive.Escalate;
                }
            }));
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new LobbySupervisorActor());
}
