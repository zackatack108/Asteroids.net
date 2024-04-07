namespace Asteroids.Shared;

public class Map
{
    public required Guid MapId { get; init; }
    public required List<Player> Players { get; set; }
    public required List<Asteroid> Asteroids { get; set; }
    public required MapState State { get; set; }
    public required int Height { get; init; }
    public required int Width { get; init; }

    public Map(List<Player> players, int height, int width)
    {
        MapId = new();
        Players = players;
        Height = height;
        Width = width;
        State = MapState.JOINING;
        Asteroids = new();
    }
}
