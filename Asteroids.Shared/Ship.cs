namespace Asteroids.Shared;

public class Ship
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Heading { get; set; }
    public double Health { get; set; }
    public Weapon? Weapon { get; set; }
    public int TurnSpeed { get; set; } = 10;

}
