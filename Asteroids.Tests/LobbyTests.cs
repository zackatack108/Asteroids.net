using Akka.Actor;
using Akka.TestKit.NUnit;
using Asteroids.API.Actors;
using Asteroids.API.Services;
using Asteroids.Shared;
using FluentAssertions;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.Tests;

[TestFixture]
public class LobbyTests : TestKit
{

    SignalRService signalRService = new("http://test");

    [Test]
    public void Player_is_able_to_join_lobby()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var player = new Player { Username = "zack", Bank = 0, Score = 0, Ship = new() };
        var joinLobby = new LobbyJoinMessage(guid, player.Username);

        lobbyActor.Tell(joinLobby, probe.Ref);
        var response = probe.ExpectMsg<LobbyJoinResponse>();
        response.lobby.LobbyId.Should().Be(guid);
        response.lobby.Map.Players[0].Username.Should().Be(player.Username);
    }

    //[Test]
    //public void Lobby_should_send_error_message_if_no_player_is_found()
    //{
    //    var guid = new Guid();
    //    var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
    //    var probe = CreateTestProbe();

    //    Player player = null;
    //    var joinLobby = new LobbyJoinMessage(guid, player.Username);

    //    lobbyActor.Tell(joinLobby, probe.Ref);
    //    var response = probe.ExpectMsg<LobbyErrorResponse>();
    //    response.errorMsg.Should().Be("Player not found");
    //}

    [Test]
    public void Multiple_players_can_be_in_a_lobby()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var player1 = new Player { Username = "zack", Bank = 0, Score = 0, Ship = new() };
        var player2 = new Player { Username = "robotguy", Bank = 0, Score = 0, Ship = new() };
        var joinLobby1 = new LobbyJoinMessage(guid, player1.Username);
        var joinLobby2 = new LobbyJoinMessage(guid, player2.Username);

        lobbyActor.Tell(joinLobby1, probe.Ref);
        var response1 = probe.ExpectMsg<LobbyJoinResponse>();

        lobbyActor.Tell(joinLobby2, probe.Ref);
        var response2 = probe.ExpectMsg<LobbyJoinResponse>();

        response1.lobby.LobbyId.Should().Be(guid);
        response1.lobby.Map.Players[0].Username.Should().Be(player1.Username);
        response2.lobby.LobbyId.Should().Be(guid);
        response2.lobby.Map.Players[1].Username.Should().Be(player2.Username);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_active()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var newState = new LobbyChangeStateMessage(guid, LobbyState.ACTIVE);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.state.Should().Be(LobbyState.ACTIVE);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_inactive()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var newState = new LobbyChangeStateMessage(guid, LobbyState.INACTIVE);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.state.Should().Be(LobbyState.INACTIVE);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_resetting()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var newState = new LobbyChangeStateMessage(guid, LobbyState.RESETTING);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.state.Should().Be(LobbyState.JOINING);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_joining()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid, false, null, signalRService));
        var probe = CreateTestProbe();

        var newState = new LobbyChangeStateMessage(guid, LobbyState.JOINING);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.state.Should().Be(LobbyState.JOINING);
    }

}
