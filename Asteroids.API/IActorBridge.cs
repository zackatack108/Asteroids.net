using Akka.Actor;

namespace Asteroids.API;

public interface IActorBridge
{
    void Tell(IActorRef actorRef, object message);
    Task<T> Ask<T>(IActorRef actorRef, object message);
}
