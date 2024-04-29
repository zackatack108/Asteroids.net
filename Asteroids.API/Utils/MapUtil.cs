using Akka.Actor;
using Asteroids.Shared;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.API.Utils;

public class MapUtil
{
    private Map map;
    IActorRef actor;
    IActorRef supervisor;

    public MapUtil(Map map, IActorRef actor, IActorRef supervisor)
    {
        this.map = map;
        this.actor = actor;
        this.supervisor = supervisor;
    }

    public void HandleShipMovement(LobbyMovePlayerMessage moveMessage)
    {
        if (moveMessage.player == null || !map.Players.Contains(moveMessage.player))
        {
            actor.Tell(new LobbyErrorResponse("Can't find player"));
            return;
        }

        if (moveMessage.direction.TurnLeft && !moveMessage.direction.TurnRight)
        {
            moveMessage.player.Ship.Heading -= moveMessage.player.Ship.TurnSpeed;
        }
        else if (moveMessage.direction.TurnRight && !moveMessage.direction.TurnLeft)
        {
            moveMessage.player.Ship.Heading += moveMessage.player.Ship.TurnSpeed;
        }

        moveMessage.player.Ship.Heading = (moveMessage.player.Ship.Heading + 360) % 360;

        if (moveMessage.direction.MoveForward && !moveMessage.direction.MoveBackward)
        {
            double angleInRadians = moveMessage.player.Ship.Heading * (Math.PI / 180);
            double deltaX = Math.Cos(angleInRadians);
            double deltaY = Math.Sin(angleInRadians);

            moveMessage.player.Ship.PositionX += (int)Math.Round(deltaX);
            moveMessage.player.Ship.PositionY += (int)Math.Round(deltaY);
        }
        else if (moveMessage.direction.MoveBackward && !moveMessage.direction.MoveForward)
        {
            double angleInRadians = moveMessage.player.Ship.Heading * (Math.PI / 180);
            double deltaX = -Math.Cos(angleInRadians);
            double deltaY = -Math.Sin(angleInRadians);

            moveMessage.player.Ship.PositionX += (int)Math.Round(deltaX);
            moveMessage.player.Ship.PositionY += (int)Math.Round(deltaY);
        }
    }

    public Map GetMap()
    {
        return map;
    }

    public void ChangeMapSize(int height, int width)
    {
        map.Width = width;
        map.Height = height;
    }

    public void RunGameTick()
    {
        Random random = new Random();

        int minX = 0;
        int maxX = map.Width;
        int minY = 0;
        int maxY = map.Height;

        List<Asteroid> newAsteroids = new List<Asteroid>();

        foreach (var asteroid in map.Asteroids.ToList())
        {
            double angleInRadians = asteroid.Heading * (Math.PI / 180);
            asteroid.PositionX += (int)Math.Round(Math.Cos(angleInRadians));
            asteroid.PositionY += (int)Math.Round(Math.Sin(angleInRadians));

            foreach (var player in map.Players)
            {
                if (IsCollision(asteroid, player))
                {
                    double damage = (asteroid.DamagePercent / 100.0) * player.Ship.Health;
                    player.Ship.Health -= damage;

                    if(player.Ship.Health <= 0)
                    {
                        map.Players.Remove(player);
                    }
                }
            }

            if (asteroid.PositionX < minX || asteroid.PositionX > maxX ||
                asteroid.PositionY < minY || asteroid.PositionY > maxY)
            {
                map.Asteroids.Remove(asteroid);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            Asteroid newAsteroid = new Asteroid
            {
                PositionX = random.Next(minX, maxX + 1),
                PositionY = random.Next(minY, maxY + 1),
                Heading = random.Next(0, 360),
                Size = random.Next(1, 4),
                Health = 100.0
            };

            newAsteroids.Add(newAsteroid);
        }

        map.Asteroids.AddRange(newAsteroids);

        actor.Tell(new LobbyMapResponse(map));
    }

    private bool IsCollision(Asteroid asteroid, Player player)
    {
        int distanceSquared = (asteroid.PositionX - player.Ship.PositionX) * (asteroid.PositionX - player.Ship.PositionX)
                            + (asteroid.PositionY - player.Ship.PositionY) * (asteroid.PositionY - player.Ship.PositionY);

        int collisionRadius = 10; 
        return distanceSquared <= collisionRadius * collisionRadius;
    }
}
