using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class MapMessages
{

    public record MovePlayerMessage(Guid MapId, Player Player, MovementDirection Direction);
    public record GetMapMessage(Guid MapId);
    public record UpdateMap();
    public record GetMapResponse(Map map);
    public record MapErrorResponse(string ErrorMessage);
}
