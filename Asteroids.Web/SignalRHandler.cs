using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace Asteroids.Web;

public class SignalRHandler
{
    private readonly HubConnection hubConnection;

    public event Action<Guid, LobbyState>? LobbyStateReceived;
    public event Action<List<Lobby>>? LobbyListReceived;
    public event Action<Lobby>? LobbyInfoReceived;
    public event Action<Map>? MapInfoReceived;

    public SignalRHandler(string hubURL)
    {
        hubConnection = new HubConnectionBuilder().WithUrl(hubURL).Build();

        hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5));
            await hubConnection.StartAsync();
        };

        RegisterMessageHandlers();

        _ = StartConnectionAsync();
    }

    private async Task StartConnectionAsync()
    {
        try
        {
            await hubConnection.StartAsync();
            Console.WriteLine("SignalR connected");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
        }
    }

    private void RegisterMessageHandlers()
    {
        hubConnection.On<Guid, LobbyState>("LobbyStateResponse", (lobbyId, state) =>
        {
            LobbyStateReceived?.Invoke(lobbyId, state);
        });

        hubConnection.On<List<Lobby>>("LobbyList", (lobbies) =>
        {
            LobbyListReceived?.Invoke(lobbies);
        });

        hubConnection.On<Lobby>("LobbyInfoResponse", (lobby) =>
        {
            LobbyInfoReceived?.Invoke(lobby);
        });

        hubConnection.On<Map>("MapInfoResponse", (map) =>
        {
            MapInfoReceived?.Invoke(map);
        });
    }

    //create Lobby
    public async Task CreateLobby()
    {
        try
        {
            await hubConnection.SendAsync("CreateLobby");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending create lobby request: {ex.Message}");
        }
    }

    //Kill Lobby
    public async Task KillLobby(Guid lobbyId)
    {
        try
        {
            await hubConnection.SendAsync("KillLobby", lobbyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending kill lobby request: {ex.Message}");
        }
    }

    //Join Lobby
    public async Task JoinLobby(Guid lobbyId, string username)
    {
        try
        {
            await hubConnection.SendAsync("JoinLobby", lobbyId, username);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending join lobby request: {ex.Message}");
        }
    }

    //Change lobby state
    public async Task ChangeLobbyState(Guid lobbyId, LobbyState state)
    {
        try
        {
            await hubConnection.SendAsync("ChangeLobbyState", lobbyId, state);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending change lobby state request: {ex.Message}");
        }
    }

    //Get Lobby State
    public async Task GetLobbyState(Guid lobbyId)
    {
        try
        {
            await hubConnection.SendAsync("GetLobbyState", lobbyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending lobby state request: {ex.Message}");
        }
    }

    //Get Lobbies
    public async Task GetLobbies()
    {
        try
        {
            await hubConnection.SendAsync("GetLobbies");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending get lobbies request: {ex.Message}");
        }
    }

    //Get Lobby Info
    public async Task GetLobbyInfo(Guid lobbyId)
    {
        try
        {
            await hubConnection.SendAsync("GetLobbyInfo", lobbyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending lobby info request: {ex.Message}");
        }
    }

    //Modify Map size
    public async Task ModifyMapSize(Guid lobbyId, int height, int width)
    {
        try
        {
            await hubConnection.SendAsync("ModifyMapSize", lobbyId, height, width);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending map size modification request: {ex.Message}");
        }
    }

    //Map Move player
    public async Task MapMovePlayer(Guid lobbyId, Player player, MovementDirection direction)
    {
        try
        {
            await hubConnection.SendAsync("MapMovePlayerMessage", lobbyId, player, direction);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending map move player request: {ex.Message}");
        }
    }

    //Get Map info
    public async Task GetMapInfo(Guid lobbyId)
    {
        try
        {
            await hubConnection.SendAsync("GetMapInfo", lobbyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending get map info request: {ex.Message}");
        }
    }

}
