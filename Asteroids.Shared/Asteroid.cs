using System.Drawing;

namespace Asteroids.Shared;

public class Asteroid
{
    public Guid AsteroidId { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Heading { get; set; }
    public int Size { get; set; }
    public double Health { get; set; }

    public double DamagePercent
    {
        get
        {
            return Size switch
            {
                1 => 10.0, 
                2 => 20.0,
                3 => 30.0,
                4 => 40.0,
                5 => 50.0,
                _ => 10.0,
            };
        }
    }

    public Asteroid()
    {
        AsteroidId = Guid.NewGuid();
        Health = 100.0;
    }
}
