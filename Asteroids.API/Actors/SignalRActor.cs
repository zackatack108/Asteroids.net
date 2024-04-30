using Akka.Actor;
using Asteroids.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using static Asteroids.API.Messages.LobbyMessages;
using static Asteroids.API.Messages.LobbySupervisorMessages;

namespace Asteroids.API.Actors;

public class SignalRActor : ReceiveActor
{

    private HubConnection hubConnection;
    private readonly IActorRef lobbySupervisor;

    public SignalRActor(IActorRef lobbySupervisor, string hubUrl)
    {
        this.lobbySupervisor = lobbySupervisor;
        hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).Build();
        Console.WriteLine($"SignalR hub URL: {hubUrl}");

        hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5));
            await hubConnection.StartAsync();
        };

        _ = OpenConnectionAsync();

        Receive<LobbyStateResponse>(SendLobbyStateResponse);
        Receive<LobbyListResponse>(SendLobbyListResponse);
        Receive<LobbyInfoResponse>(SendLobbyInfoResponse);
        Receive<LobbyMapResponse>(SendMapInfoResponse);

        RegisterMessageHandlers();
    }

    private async Task OpenConnectionAsync()
    {
        try
        {
            await hubConnection.StartAsync();
            Console.WriteLine("SignalR connection established");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
        }
    }

    private void RegisterMessageHandlers()
    {
        hubConnection.On("CreateLobby", () =>
        {
            Console.WriteLine("Creating a new lobby");
            lobbySupervisor.Ask(new LobbyCreateMessage());
        });

        hubConnection.On<Guid>("KillLobby", (lobbyId) =>
        {
            lobbySupervisor.Ask(new LobbyTerminateMessage(lobbyId));
        });

        hubConnection.On<Guid, Player>("JoinLobby", (lobbyId, player) =>
        {
            lobbySupervisor.Ask(new LobbyJoinMessage(lobbyId, player));
        });

        hubConnection.On<Guid, LobbyState>("ChangeLobbyState", (lobbyId, state) =>
        {
            lobbySupervisor.Ask(new LobbyChangeStateMessage(lobbyId, state));
        });

        hubConnection.On<Guid>("GetLobbyState", (lobbyId) =>
        {
            lobbySupervisor.Ask(new LobbyCurrentStateMessage(lobbyId));
        });

        hubConnection.On("GetLobbies", () =>
        {
            lobbySupervisor.Ask(new LobbyListMessage());
        });

        hubConnection.On<Guid>("GetLobbyInfo", (lobbyId) =>
        {
            lobbySupervisor.Ask(new LobbyInfoMessage(lobbyId));
        });

        hubConnection.On<Guid, int, int>("ModifyMapSize", (lobbyId, height, width) =>
        {
            lobbySupervisor.Ask(new LobbySetMapSizeMessage(lobbyId, height, width));
        });

        hubConnection.On<Guid, Player, MovementDirection>("MapMovePlayerMessage", (lobbyId, player, direction) =>
        {
            lobbySupervisor.Ask(new LobbyMovePlayerMessage(lobbyId, player, direction));
        });

        hubConnection.On<Guid>("GetMapInfo", (lobbyId) =>
        {
            lobbySupervisor.Ask(new LobbyGetMapMessage(lobbyId));
        });
    }

    private void SendLobbyStateResponse(LobbyStateResponse response)
    {
        try
        {
            hubConnection.SendAsync("LobbyStateResponse", response.lobbyId, response.state);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending LobbyStateResponse to SignalR Hub: {ex.Message}");
        }
    }

    private void SendLobbyListResponse(LobbyListResponse response)
    {
        try
        {
            hubConnection.SendAsync("LobbyListResponse", response.lobbies);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending LobbyListResponse to SignalR Hub: {ex.Message}");
        }
    }

    private void SendLobbyInfoResponse(LobbyInfoResponse response)
    {
        try
        {
            hubConnection.SendAsync("LobbyInfoResponse", response.lobby);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending LobbyInfoResponse to SignalR Hub: {ex.Message}");
        }
    }

    private void SendMapInfoResponse(LobbyMapResponse response)
    {
        try
        {
            hubConnection.SendAsync("MapInfoResponse", response.map);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending MapInfoResponse to SignalR Hub: {ex.Message}");
        }
    }

    protected override void PostStop()
    {
        hubConnection.StopAsync().Wait();
        base.PostStop();
    }

    public static Props Props(IActorRef lobbySupervisor, string hubUrl)
            => Akka.Actor.Props.Create(() => new SignalRActor(lobbySupervisor, hubUrl));
}
