using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class LobbyMessages
{
    //Receiving messages
    public record LobbyJoinMessage(Guid lobbyId, Player player);
    public record LobbyChangeStateMessage(Guid lobbyId, LobbyState state);
    public record LobbyCurrentStateMessage(Guid lobbyId);
    public record LobbyInfoMessage(Guid lobbyId);

    //Response messages
    public record LobbyJoinResponse(Guid lobbyId, List<Player> players);
    public record LobbyStateResponse(Guid lobbyId, LobbyState state);
    public record LobbyInfoResponse(Lobby lobby);
    public record LobbyErrorResponse(string ErrorMsg);

}
