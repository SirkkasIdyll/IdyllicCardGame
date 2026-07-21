using GdUnit4;
using Godot;
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
    
    
}