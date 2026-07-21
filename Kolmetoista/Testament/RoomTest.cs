using System.Threading.Tasks;
using GdUnit4;
using Godot;
using Kolmetoista.Systems.Player;
using Kolmetoista.Systems.Rules;
using Kolmetoista.Temperance.NCS;
using static GdUnit4.Assertions;

namespace Kolmetoista.Testament;

[TestSuite][RequireGodotRuntime]
public class RoomTest
{
    private readonly Node _rootScene = new();
    private readonly ComponentManager _componentManager = ComponentManager.Instance;
    private readonly NodeManager _nodeManager = NodeManager.Instance;
    private readonly NodeSystemManager _nodeSystemManager = NodeSystemManager.Instance;

    [Before]
    public void Before()
    {
        _rootScene.TreeEntered += () =>
        {
            _nodeSystemManager.InitializeNodeSystems(_rootScene);
            _nodeManager.SetRootScene(_rootScene);
        };
        AddNode(_rootScene);
    }

    [TestCase]
    public void JoinAndLeaveRoomTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RoomSystem>(out var roomSystem)).IsTrue();
        if (roomSystem == null)
            return;

        // Create a new player 
        var player = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player, out var playerHandComponent))
            return;
        
        // You're only allowed to join once
        AssertBool(roomSystem.TryJoinRoom((player, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player, playerHandComponent))).IsFalse();

        roomSystem.LeaveRoom((player, playerHandComponent));
        
        // If you left a room, you should be able to rejoin it
        AssertBool(roomSystem.TryJoinRoom((player, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player, playerHandComponent))).IsFalse();
    }

    [TestCase]
    public void JoinMaxCapacityRoomTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RoomSystem>(out var roomSystem)).IsTrue();
        if (roomSystem == null)
            return;

        // Create a new player 
        var player = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player, out var playerHandComponent))
            return;
        
        // Create a new player 
        var player2 = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player2)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player2, out var player2HandComponent))
            return;
        
        // Create a new player 
        var player3 = new Node();
        AddNode(player3);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player3)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player3, out var player3HandComponent))
            return;
        
        // Create a new player 
        var player4 = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player4)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player4, out var player4HandComponent))
            return;
        
        // Create a new player 
        var player5 = new Node();
        AddNode(player5);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player5)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player5, out var player5HandComponent))
            return;
        
        // Max capacity is 4, so the fifth player should be rejected
        AssertBool(roomSystem.TryJoinRoom((player, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player2, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player3, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player4, playerHandComponent))).IsTrue();
        AssertBool(roomSystem.TryJoinRoom((player5, playerHandComponent))).IsFalse();

        roomSystem.LeaveRoom((player2, player2HandComponent));
        AssertBool(roomSystem.TryJoinRoom((player5, player5HandComponent))).IsTrue();
    }
}