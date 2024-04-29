using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Asteroids.API.Actors;
using Asteroids.Shared;
using FluentAssertions;
using static Asteroids.API.Messages.LobbyMessages;
using static Asteroids.API.Messages.LobbySupervisorMessages;

namespace Asteroids.Tests;

public class LobbySupervisorTests : TestKit
{
    public Guid Setup(IActorRef lobbySupervisor, TestProbe probe)
    {    
        lobbySupervisor.Tell(new LobbyCreateMessage(), probe.Ref);

        var lobbyList = probe.ExpectMsg<LobbyListResponse>(TimeSpan.FromSeconds(1));
        return lobbyList.lobbies[0].LobbyId;
    }

    [Test]
    public void Able_to_create_a_new_lobby()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();

        lobbySupervisor.Tell(new LobbyCreateMessage(), probe.Ref);

        var response = probe.ExpectMsg<LobbyListResponse>(TimeSpan.FromSeconds(1));
        response.lobbies.Count.Should().Be(1);
    }

    [Test]
    public void Able_to_create_multiple_lobbies()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();

        lobbySupervisor.Tell(new LobbyCreateMessage(), probe.Ref);
        probe.ExpectMsg<LobbyListResponse>(TimeSpan.FromSeconds(1));
        lobbySupervisor.Tell(new LobbyCreateMessage(), probe.Ref);

        var response = probe.ExpectMsg<LobbyListResponse>(TimeSpan.FromSeconds(1));
        response.lobbies.Count.Should().Be(2);
    }

    [Test]
    public void Able_to_get_info_about_a_lobby()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbyInfoMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyInfoResponse>(TimeSpan.FromSeconds(1));
        response.lobby.LobbyId.Should().Be(lobbyId);
        response.lobby.State.Should().Be(LobbyState.JOINING);
        response.lobby.Map.Players.Count.Should().Be(0);
    }

    [Test]
    public void Player_is_able_to_join_a_lobby()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players.Count.Should().Be(1);
        response.map.Players[0].Should().Be(player);
    }

    [Test]
    public void Multiple_players_can_join_a_lobby()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship1 = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player1 = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship1 };

        Ship ship2 = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player2 = new Player { Username = "Cody", Bank = 0, Score = 0, Ship = ship2 };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player1), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player2), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players.Count.Should().Be(2);
        response.map.Players[0].Should().Be(player1);
        response.map.Players[1].Should().Be(player2);
    }

    [Test]
    public void Map_size_is_able_to_be_set()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbySetMapSizeMessage(lobbyId, 500, 500), probe.Ref);

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Height.Should().Be(500);
        response.map.Width.Should().Be(500);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_active()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        lobbySupervisor.Tell(new LobbyCurrentStateMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        response.state.Should().Be(LobbyState.ACTIVE);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_resetting()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.RESETTING), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        lobbySupervisor.Tell(new LobbyCurrentStateMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        response.state.Should().Be(LobbyState.JOINING);
    }

    [Test]
    public void Lobby_state_can_be_changed_to_inactive()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.INACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        lobbySupervisor.Tell(new LobbyInfoMessage(lobbyId), probe.Ref);
        probe.ExpectNoMsg(TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Player_is_able_to_move_forward()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        var direction = new MovementDirection();
        direction.MoveForward = true;
        lobbySupervisor.Tell(new LobbyMovePlayerMessage(lobbyId, player, direction), probe.Ref);

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players[0].Ship.PositionX.Should().Be(51);
        response.map.Players[0].Ship.PositionY.Should().Be(50);
    }

    [Test]
    public void Player_is_able_to_move_backward()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        var direction = new MovementDirection();
        direction.MoveBackward = true;
        lobbySupervisor.Tell(new LobbyMovePlayerMessage(lobbyId, player, direction), probe.Ref);

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players[0].Ship.PositionX.Should().Be(49);
        response.map.Players[0].Ship.PositionY.Should().Be(50);
    }

    [Test]
    public void Player_is_able_to_turn_left()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        var direction = new MovementDirection();
        direction.TurnLeft = true;
        lobbySupervisor.Tell(new LobbyMovePlayerMessage(lobbyId, player, direction), probe.Ref);

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players[0].Ship.Heading.Should().Be(350);
    }

    [Test]
    public void Player_is_able_to_turn_right()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        lobbySupervisor.Tell(new LobbyJoinMessage(lobbyId, player), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>(TimeSpan.FromSeconds(3));

        lobbySupervisor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>(TimeSpan.FromSeconds(1));

        var direction = new MovementDirection();
        direction.TurnRight = true;
        lobbySupervisor.Tell(new LobbyMovePlayerMessage(lobbyId, player, direction), probe.Ref);

        lobbySupervisor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>(TimeSpan.FromSeconds(1));

        response.map.Players[0].Ship.Heading.Should().Be(10);
    }

    [Test]
    public void Lobby_will_be_restarted_when_it_crashes()
    {
        var lobbySupervisor = this.Sys.ActorOf(LobbySupervisorActor.Props());
        var probe = CreateTestProbe();
        var lobbyId = Setup(lobbySupervisor, probe);

        lobbySupervisor.Tell(new LobbyInfoMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyInfoResponse>(TimeSpan.FromSeconds(1));

        response.lobby.LobbyId.Should().Be(lobbyId);

        lobbySupervisor.Tell(new LobbyCrashMessage(lobbyId), probe.Ref);
        probe.ExpectNoMsg(TimeSpan.FromSeconds(1));
        lobbySupervisor.Tell(new LobbyInfoMessage(lobbyId), probe.Ref);
        response = probe.ExpectMsg<LobbyInfoResponse>(TimeSpan.FromSeconds(3));

        response.lobby.LobbyId.Should().Be(lobbyId);

    }
}
