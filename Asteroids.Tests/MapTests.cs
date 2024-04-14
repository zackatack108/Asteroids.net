
using Akka.TestKit.NUnit;
using Asteroids.API.Actors;
using Asteroids.Shared;
using FluentAssertions;
using static Asteroids.API.Messages.MapMessages;

namespace Asteroids.Tests;

public class MapTests : TestKit
{
    [Test]
    public void Players_are_added_to_map_on_creation()
    {
        var mapId = new Guid();

        List<Player> players = new();
        players.Add(new Player { Username = "Zack", Bank = 0, Score = 0, Ship = null });

        var mapActor = this.Sys.ActorOf(MapActor.Props(mapId, 100, 100, MapState.JOINING, players));
        var probe = CreateTestProbe();

        mapActor.Tell(new GetMapMessage(mapId), probe.Ref);
        var response = probe.ExpectMsg<GetMapResponse>();

        response.map.MapId.Should().Be(mapId);
        response.map.Players.Should().BeEquivalentTo(players);
    }

    [Test]
    public void Player_ship_is_able_to_move_forward()
    {
        var mapId = new Guid();

        List<Player> players = new();
        Ship ship = new Ship{ PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };
        players.Add(player);


        var mapActor = this.Sys.ActorOf(MapActor.Props(mapId, 100, 100, MapState.JOINING, players));
        var probe = CreateTestProbe();

        mapActor.Tell(new MovePlayerMessage(mapId, player, new MovementDirection { MoveForward = true}), probe.Ref);

        mapActor.Tell(new GetMapMessage(mapId), probe.Ref);
        var response = probe.ExpectMsg<GetMapResponse>();

        response.map.MapId.Should().Be(mapId);
        response.map.Players[0].Ship.PositionX.Should().Be(51);
        response.map.Players[0].Ship.PositionY.Should().Be(50);
    }

    [Test]
    public void Player_ship_is_able_to_turn_left()
    {
        var mapId = new Guid();

        List<Player> players = new();
        Ship ship = new Ship { PositionX = 50, PositionY = 50, Heading = 0, Health = 100 };
        Player player = new Player { Username = "Zack", Bank = 0, Score = 0, Ship = ship };
        players.Add(player);


        var mapActor = this.Sys.ActorOf(MapActor.Props(mapId, 100, 100, MapState.JOINING, players));
        var probe = CreateTestProbe();

        mapActor.Tell(new MovePlayerMessage(mapId, player, new MovementDirection { TurnLeft = true }), probe.Ref);

        mapActor.Tell(new GetMapMessage(mapId), probe.Ref);
        var response = probe.ExpectMsg<GetMapResponse>();

        response.map.MapId.Should().Be(mapId);
        response.map.Players[0].Ship.Heading.Should().Be(350);
    }
}
