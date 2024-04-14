namespace Asteroids.Shared;

public class Lobby
{
    public Guid LobbyId { get; init; }
    public List<Player> Players { get; set; } = new();
    public LobbyState State { get; set; } = LobbyState.JOINING;
    public Map? Map { get; set; }

}
