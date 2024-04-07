using Asteroids.Shared;

namespace Asteroids.API;

public class Records
{
    public record JoinLobby(Guid lobbyId, Player player);
    public record ChangeLobbyState(Guid LobbyId, LobbyState state);
    public record PlayerJoined(Player player);
    public record CreateLobby;
    public record GetLobbyState;
}
