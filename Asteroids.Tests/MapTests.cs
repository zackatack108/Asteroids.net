
using Akka.TestKit.NUnit;
using Asteroids.API.Actors;
using Asteroids.API.Services;
using Asteroids.Shared;
using FluentAssertions;
using System;
using static Asteroids.API.Messages.LobbyMessages;

namespace Asteroids.Tests;

public class MapTests : TestKit
{

    SignalRService signalRService = new("http://test");

    [Test]
    public void Player_ship_is_able_to_move_forward()
    {
        var lobbyId = new Guid();
        Ship ship = new Ship{ PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };
        
        var actor = this.Sys.ActorOf(LobbyActor.Props(lobbyId, false, null, signalRService));
        var probe = CreateTestProbe();

        actor.Tell(new LobbySetMapSizeMessage(lobbyId, 100, 100), probe.Ref);
        actor.Tell(new LobbyJoinMessage(lobbyId, player.Username), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>();

        actor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>();

        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var map = probe.ExpectMsg<LobbyMapResponse>();
        player = map.map.Players[0];
        int xpos = player.Ship.PositionX;

        actor.Tell(new LobbyMovePlayerMessage(lobbyId, player, new MovementDirection { MoveForward = true}), probe.Ref);
        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>();

        response.map.Players[0].Ship.PositionX.Should().Be(xpos+1);
        response.map.Players[0].Ship.PositionY.Should().Be(player.Ship.PositionY);
    }

    [Test]
    public void Player_ship_is_able_to_move_backward()
    {
        var lobbyId = new Guid();
        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        var actor = this.Sys.ActorOf(LobbyActor.Props(lobbyId, false, null, signalRService));
        var probe = CreateTestProbe();

        actor.Tell(new LobbySetMapSizeMessage(lobbyId, 100, 100), probe.Ref);
        actor.Tell(new LobbyJoinMessage(lobbyId, player.Username), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>();

        actor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>();

        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var map = probe.ExpectMsg<LobbyMapResponse>();
        player = map.map.Players[0];
        int xpos = player.Ship.PositionX;

        actor.Tell(new LobbyMovePlayerMessage(lobbyId, player, new MovementDirection { MoveBackward = true }), probe.Ref);
        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>();

        response.map.Players[0].Ship.PositionX.Should().Be(xpos-1);
        response.map.Players[0].Ship.PositionY.Should().Be(player.Ship.PositionY);
    }

    [Test]
    public void Player_ship_is_able_to_turn_left()
    {
        var lobbyId = new Guid();

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        var actor = this.Sys.ActorOf(LobbyActor.Props(lobbyId, false, null, signalRService));
        var probe = CreateTestProbe();

        actor.Tell(new LobbySetMapSizeMessage(lobbyId, 100, 100), probe.Ref);
        actor.Tell(new LobbyJoinMessage(lobbyId, player.Username), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>();

        actor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>();

        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var map = probe.ExpectMsg<LobbyMapResponse>();
        player = map.map.Players[0];

        actor.Tell(new LobbyMovePlayerMessage(lobbyId, player, new MovementDirection { TurnLeft = true }), probe.Ref);
        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>();

        response.map.Players[0].Ship.Heading.Should().Be(350);
    }

    [Test]
    public void Player_ship_is_able_to_turn_right()
    {
        var lobbyId = new Guid();

        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };

        var actor = this.Sys.ActorOf(LobbyActor.Props(lobbyId, false, null, signalRService));
        var probe = CreateTestProbe();

        actor.Tell(new LobbySetMapSizeMessage(lobbyId, 100, 100), probe.Ref);
        actor.Tell(new LobbyJoinMessage(lobbyId, player.Username), probe.Ref);
        probe.ExpectMsg<LobbyJoinResponse>();

        actor.Tell(new LobbyChangeStateMessage(lobbyId, LobbyState.ACTIVE), probe.Ref);
        probe.ExpectMsg<LobbyStateResponse>();

        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var map = probe.ExpectMsg<LobbyMapResponse>();
        player = map.map.Players[0];

        actor.Tell(new LobbyMovePlayerMessage(lobbyId, player, new MovementDirection { TurnRight = true }), probe.Ref);
        actor.Tell(new LobbyGetMapMessage(lobbyId), probe.Ref);
        var response = probe.ExpectMsg<LobbyMapResponse>();

        response.map.Players[0].Ship.Heading.Should().Be(10);
    }
}
