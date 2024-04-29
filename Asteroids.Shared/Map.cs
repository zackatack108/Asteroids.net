namespace Asteroids.Shared;

public class Map
{
    public List<Player> Players { get; set; } = new();
    public List<Asteroid> Asteroids { get; set; } = new();
    public int Height { get; set; } = 500;
    public int Width { get; set; } = 500;
}
