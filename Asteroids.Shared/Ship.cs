namespace Asteroids.Shared;

public class Ship
{
    public required double PositionX { get; set; }
    public required double PositionY { get; set; }
    public required int Heading { get; set; }
    public required double Health { get; set; }
    public Weapon? Weapon { get; set; }
    public int TurnSpeed { get; set; } = 10;

}
