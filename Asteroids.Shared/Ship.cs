namespace Asteroids.Shared;

public class Ship
{
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Heading { get; set; }
    public required double Health { get; set; }
    public required Weapon Weapon { get; set; }

}
