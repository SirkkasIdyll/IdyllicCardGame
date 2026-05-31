using System.Threading.Tasks;
using Godot;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Testament;

using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite][RequireGodotRuntime]
public class DeckTest
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
            _rootScene.AddChild(_nodeManager);
        };
        AddNode(_rootScene);
    }

    [TestCase]
    public async Task DeckSystemTest()
    {
    }
}