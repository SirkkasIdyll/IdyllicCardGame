using GdUnit4;
using Godot;
using Kolmetoista.Systems.Player;
using Kolmetoista.Systems.Rules;
using Kolmetoista.Temperance.NCS;
using static GdUnit4.Assertions;

namespace Kolmetoista.Testament;

[TestSuite][RequireGodotRuntime]
public class RoundTest
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
    public void StartRoundTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RoomSystem>(out var roomSystem)).IsTrue();
        if (roomSystem == null)
            return;
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RoundSystem>(out var roundSystem)).IsTrue();
        if (roundSystem == null)
            return;

        // Starting a round with no one in the room should result in it failing
        AssertBool(roundSystem.TryStartRound()).IsFalse();
        
        // Create a new player 
        var player = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player, out var playerHandComponent))
            return;
        
        // Starting a round with just one person should result in it failing
        AssertBool(roundSystem.TryStartRound()).IsFalse();
        
        // Create a new player 
        var player2 = new Node();
        AddNode(player);

        // Give them the player hand component
        AssertBool(_componentManager.TryAddComponent<PlayerHandComponent>(player2)).IsTrue();
        if (!_componentManager.TryGetComponent<PlayerHandComponent>(player2, out var player2HandComponent))
            return;
        
        AssertBool(roundSystem.TryStartRound()).IsTrue();
    }
}