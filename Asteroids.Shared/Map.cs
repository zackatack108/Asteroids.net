namespace Asteroids.Shared;

public class Map
{
    public required Guid MapId { get; init; }
    public required List<Player> Players { get; set; }
    public List<Asteroid>? Asteroids { get; set; }
    public MapState State { get; set; }
    public required int Height { get; init; }
    public required int Width { get; init; }
}
