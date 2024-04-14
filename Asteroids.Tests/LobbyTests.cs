using Akka.Actor;
using Akka.TestKit.NUnit;
using Asteroids.API.Actors;
using Asteroids.Shared;
using FluentAssertions;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.Tests;

[TestFixture]
public class LobbyTests : TestKit
{
    [Test]
    public void Player_is_able_to_join_lobby()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var player = new Player { Username = "zack", Bank = 0, Score = 0, Ship = null };
        var joinLobby = new JoinLobbyMessage(guid, player );

        lobbyActor.Tell(joinLobby, probe.Ref);
        var response = probe.ExpectMsg<JoinLobbyResponse>();
        response.lobbyId.Should().Be(guid);
        response.players[0].Should().Be(player);
    }

    [Test]
    public void Lobby_should_send_error_message_if_no_player_is_found()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        Player player = null;
        var joinLobby = new JoinLobbyMessage(guid, player);

        lobbyActor.Tell(joinLobby, probe.Ref);
        var response = probe.ExpectMsg<LobbyErrorResponse>();
        response.ErrorMsg.Should().Be("Player not found");
    }

    [Test]
    public void Multiple_players_can_be_in_a_lobby()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var player1 = new Player { Username = "zack", Bank = 0, Score = 0, Ship = null };
        var player2 = new Player { Username = "robotguy", Bank = 0, Score = 0, Ship = null };
        var joinLobby1 = new JoinLobbyMessage(guid, player1);
        var joinLobby2 = new JoinLobbyMessage(guid, player2);

        lobbyActor.Tell(joinLobby1, probe.Ref);
        var response1 = probe.ExpectMsg<JoinLobbyResponse>();

        lobbyActor.Tell(joinLobby2, probe.Ref);
        var response2 = probe.ExpectMsg<JoinLobbyResponse>();

        response1.lobbyId.Should().Be(guid);
        response1.players[0].Should().Be(player1);
        response2.lobbyId.Should().Be(guid);
        response2.players[1].Should().Be(player2);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_active()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var newState = new LobbyStateChangeMessage(guid, LobbyState.ACTIVE);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.lobbyId.Should().Be(guid);
        response.state.Should().Be(LobbyState.ACTIVE);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_inactive()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var newState = new LobbyStateChangeMessage(guid, LobbyState.INACTIVE);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.lobbyId.Should().Be(guid);
        response.state.Should().Be(LobbyState.INACTIVE);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_resetting()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var newState = new LobbyStateChangeMessage(guid, LobbyState.RESETTING);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.lobbyId.Should().Be(guid);
        response.state.Should().Be(LobbyState.JOINING);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_joining()
    {
        var guid = new Guid();
        var lobbyActor = this.Sys.ActorOf(LobbyActor.Props(guid));
        var probe = CreateTestProbe();

        var newState = new LobbyStateChangeMessage(guid, LobbyState.JOINING);

        lobbyActor.Tell(newState, probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>();

        response.lobbyId.Should().Be(guid);
        response.state.Should().Be(LobbyState.JOINING);
    }
}
