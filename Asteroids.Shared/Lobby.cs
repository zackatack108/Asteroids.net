namespace Asteroids.Shared;

public class Lobby
{
    public Guid LobbyId { get; init; }
    public LobbyState State { get; set; } = LobbyState.JOINING;
    public Map Map { get; set; } = null!;

}
