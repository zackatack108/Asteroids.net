using Akka.Actor;
using Asteroids.Shared;
using static Asteroids.API.Messages.MapMessages;

namespace Asteroids.API.Actors;

public class MapActor : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; }
    private const double TickInterval = 0.1;
    private const string RunTickTimerKey = "TickTimer";
    private Map map;

    public MapActor(Guid mapId, int height, int width, MapState state, List<Player> players)
    {
        map = new Map {MapId = mapId, Height = height, Width = width, State = state, Players = players};
        Receive<MovePlayerMessage>(HandleShipMovement);
        Receive<GetMapMessage>(HandleGettingMap);

        StartTickTimer();
    }

    public static Props Props(Guid mapId, int height, int width, MapState state, List<Player> players) => Akka.Actor.Props.Create(() => new MapActor(mapId, height, width, state, players));

    private void StartTickTimer()
    {
        Timers.StartPeriodicTimer(
            RunTickTimerKey,
            new UpdateMap(),
            TimeSpan.FromSeconds(0.05),
            TimeSpan.FromSeconds(TickInterval)
        );
    }

    private void HandleShipMovement(MovePlayerMessage moveMessage)
    {
        if(moveMessage.MapId != map.MapId)
        {
            Sender.Tell(new MapErrorResponse("Can't move player cause map Id doesn't match"));
            return;
        }

        if(moveMessage.Player == null)
        {
            Sender.Tell(new MapErrorResponse("Can't find player"));
            return;
        }

        Ship ship = moveMessage.Player.Ship;

        if(moveMessage.Direction.TurnLeft && !moveMessage.Direction.TurnRight)
        {
            ship.Heading -= ship.TurnSpeed;
        }
        else if(moveMessage.Direction.TurnRight && !moveMessage.Direction.TurnLeft)
        {
            ship.Heading += ship.TurnSpeed;
        }

        ship.Heading = (ship.Heading + 360) % 360;

        if(moveMessage.Direction.MoveForward && !moveMessage.Direction.MoveBackward)
        {
            double angleInRadians = ship.Heading * (Math.PI / 180);
            double deltaX = Math.Cos(angleInRadians);
            double deltaY = Math.Sin(angleInRadians);

            ship.PositionX += deltaX;
            ship.PositionY += deltaY;
        }
        else if (moveMessage.Direction.MoveBackward && !moveMessage.Direction.MoveForward)
        {
            double angleInRadians = ship.Heading * (Math.PI / 180);
            double deltaX = -Math.Cos(angleInRadians);
            double deltaY = -Math.Sin(angleInRadians);

            ship.PositionX += deltaX;
            ship.PositionY += deltaY;
        }

    }

    public void HandleGettingMap(GetMapMessage message)
    {
        if (message.MapId != map.MapId)
        {
            Sender.Tell(new MapErrorResponse("Error: Map Id not found"));
            return;
        }

        Sender.Tell(new GetMapResponse(map));
    }
}
