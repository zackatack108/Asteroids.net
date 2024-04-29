using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class LobbyMessages
{
    //Receiving messages
    public record LobbyJoinMessage(Guid lobbyId, Player player);
    public record LobbyChangeStateMessage(Guid lobbyId, LobbyState state);
    public record LobbyCurrentStateMessage(Guid lobbyId);
    public record LobbyInfoMessage(Guid lobbyId);
    public record LobbyMovePlayerMessage(Guid lobbyId, Player player, MovementDirection direction);
    public record LobbyGetMapMessage(Guid lobbyId);
    public record LobbySetMapSizeMessage(Guid lobbyId, int height, int width);
    public record LobbyUpdateMapMessage();
    public record LobbyCrashMessage(Guid lobbyId);

    //Response messages
    public record LobbyJoinResponse(Lobby lobby);
    public record LobbyStateResponse(LobbyState state);
    public record LobbyInfoResponse(Lobby lobby);
    public record LobbyMapResponse(Map map);
    public record LobbyErrorResponse(string errorMsg);

}
