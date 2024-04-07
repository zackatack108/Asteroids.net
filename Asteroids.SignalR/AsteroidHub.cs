using Microsoft.AspNetCore.SignalR;

namespace Asteroids.SignalR;

public class AsteroidHub : Hub
{
    public async Task JoinLobby(Guid lobbyId, string username)
    {
        await Clients.All.SendAsync("JoinLobby", lobbyId, username);
    }

    public async Task ChangeLobbyState(Guid lobbyId, string state)
    {
        await Clients.All.SendAsync("ChangeLobbyState", lobbyId, state);
    }

    public async Task GetLobbyState(Guid lobbyId)
    {
        await Clients.All.SendAsync("GetLobbyState", lobbyId);
    }

    public async Task LobbyState(Guid lobbyId, string state)
    {
        await Clients.All.SendAsync("LobbyState", lobbyId, state);
    }
}
