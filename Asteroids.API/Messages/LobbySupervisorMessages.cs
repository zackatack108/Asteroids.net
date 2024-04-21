using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class LobbySupervisorMessages
{

    //Received Messages
    public record LobbyCreateMessage();
    public record LobbyListMessage();
    public record LobbyTerminateMessage(Guid lobbyId);
    
    //Response Messages
    public record LobbyCreateResponse(string message);
    public record LobbyListResponse(List<Lobby> lobbies);
    public record LobbyTerminateResponse(string message);
    public record LobbySupervisorErrorResponse(string errorMessage);
}
