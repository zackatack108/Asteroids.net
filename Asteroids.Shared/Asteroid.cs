namespace Asteroids.Shared;

public class Asteroid
{
    public required Guid AsteroidId { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Damage { get; set; }
    public required double Health { get; set; }
}
