namespace Asteroids.Shared;

public class Lobby
{
    public Guid Session { get; init; }
    public List<Player> Players { get; set; }
    public LobbyState State { get; set; }
    public Map? Map { get; set; }

    public Lobby()
    {
        Session = new();
        Players = new List<Player>();
        State = LobbyState.JOINING;
    }

}
