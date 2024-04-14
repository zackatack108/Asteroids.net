namespace Asteroids.Shared;

public class Asteroid
{
    public required Guid AsteroidId { get; set; }
    public required int PositionX { get; set; }
    public required int PositionY { get; set; }
    public required int Heading { get; set; }
    public required double DamagePercent { get; set; }
    public required double Health { get; set; }
}
