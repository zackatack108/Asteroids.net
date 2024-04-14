using Asteroids.Shared;

namespace Asteroids.API.Messages;

public class LobbySupervisorMessages
{

    //Received Messages
    public record CreateLobbyMessage();
    public record ListLobbyMessage();
    public record TerminateLobbyMessage(Guid lobbyId);
    
    //Response Messages
    public record CreateLobbyResponse(string message);
    public record ListLobbyResponse(List<Lobby> lobbies);
    public record TerminateLobbyResponse(string message);
    public record LobbySupervisorErrorResponse(string errorMessage);
}
