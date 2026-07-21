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
    public async Task JoinAndLeaveRoomTest()
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
}