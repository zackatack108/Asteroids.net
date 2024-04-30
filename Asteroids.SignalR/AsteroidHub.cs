using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Asteroids.SignalR;

public class AsteroidHub : Hub
{
    public async Task CreateLobby()
    {
        await Clients.All.SendAsync("CreateLobby");
    }

    public async Task KillLobby(Guid lobbyId)
    {
        await Clients.All.SendAsync("KillLobby", lobbyId);
    }

    public async Task JoinLobby(Guid lobbyId, Player player)
    {
        await Clients.All.SendAsync("JoinLobby", lobbyId, player);
    }

    public async Task ChangeLobbyState(Guid lobbyId, LobbyState state)
    {
        await Clients.All.SendAsync("ChangeLobbyState", lobbyId, state);
    }

    public async Task GetLobbyState(Guid lobbyId)
    {
        await Clients.All.SendAsync("GetLobbyState", lobbyId);
    }

    public async Task LobbyStateResponse(Guid lobbyId, LobbyState state)
    {
        await Clients.All.SendAsync("LobbyStateResponse", lobbyId, state);
    }

    public async Task GetLobbies()
    {
        await Clients.All.SendAsync("GetLobbies");
    }

    public async Task LobbyListResponse(List<Lobby> lobbies)
    {
        await Clients.All.SendAsync("LobbyList", lobbies);
    }

    public async Task GetLobbyInfo(Guid lobbyId)
    {
        await Clients.All.SendAsync("GetLobbyInfo", lobbyId);
    }

    public async Task LobbyInfoResponse(Lobby lobby)
    {
        await Clients.All.SendAsync("LobbyInfoResponse", lobby);
    }

    public async Task ModifyMapSize(Guid lobbyId, int height, int width)
    {
        await Clients.All.SendAsync("ModifyMapSize", height, width);
    }

    public async Task MapMovePlayerMessage(Guid lobbyId, Player player, MovementDirection direction)
    {
        await Clients.All.SendAsync("MovePlayer", lobbyId, player, direction);
    }

    public async Task GetMapInfo(Guid lobbyId)
    {
        await Clients.All.SendAsync("GetMapInfo", lobbyId);
    }

    public async Task MapInfoResponse(Map map)
    {
        await Clients.All.SendAsync("MapInfoResponse", map);
    }
}
