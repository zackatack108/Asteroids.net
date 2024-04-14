using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class LobbyMessages
{
    //Receiving messages
    public record JoinLobbyMessage(Guid lobbyId, Player player);
    public record LobbyStateChangeMessage(Guid lobbyId, LobbyState state);
    public record LobbyStateMessage(Guid lobbyId);

    //Response messages
    public record JoinLobbyResponse(Guid lobbyId, List<Player> players);
    public record LobbyStateResponse(Guid lobbyId, LobbyState state);
    public record LobbyErrorResponse(string ErrorMsg);

}
