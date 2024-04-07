namespace Asteroids.Shared;

public class Player
{
    public required string Username { get; set; }
    public required int Score { get; set; }
    public required decimal Bank { get; set; }
    public required Ship Ship { get; set; }
}
