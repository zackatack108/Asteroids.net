using Akka.Actor;
using Akka.Event;
using static Asteroids.API.Messages.LobbySupervisorMessages;

namespace Asteroids.API.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly ILoggingAdapter log = Context.GetLogger();
    private List<IActorRef>? lobbies;

    public LobbySupervisorActor()
    {
        Receive<CreateLobbyMessage>(_ => HandleCreatingLobby());
        //Receive<ListLobbyMessage>();
        //Receive<TerminateLobbyMessage>();
    }

    private void HandleCreatingLobby()
    {
        
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
