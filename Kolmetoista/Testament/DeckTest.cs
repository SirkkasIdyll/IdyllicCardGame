using System.Threading.Tasks;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Temperance.NCS;

namespace Kolmetoista.Testament;

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
            _nodeManager.SetRootScene(_rootScene);
        };
        AddNode(_rootScene);
    }

    [TestCase]
    public async Task DeckSystemTest()
    {
        _nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem);
        AssertObject(deckSystem).IsNotNull();

        if (deckSystem is null)
            return;

        deckSystem.CreateNewDeck(out var deck);
        AssertBool(deck.Count == 52).AppendFailureMessage(
            "Failed to generate a proper deck of fifty two cards."
            ).IsTrue();

        var originalDeck =  new Node<CardComponent>[52];
        deck.CopyTo(originalDeck);
        
        deckSystem.ShuffleDeck(out deck);
        var matchesOriginalDeck = true;
        for (var i = deck.Count - 1; i > 0; i--)
        {
            GD.Print(originalDeck[i].Comp.Rank + " of " + originalDeck[i].Comp.Suit + " vs " + deck[i].Comp.Rank + " of " + deck[i].Comp.Suit);

            if (originalDeck[i].Comp.Suit == deck[i].Comp.Suit && originalDeck[i].Comp.Rank == deck[i].Comp.Rank)
                continue;

            matchesOriginalDeck = false;
            break;
        }
        AssertBool(matchesOriginalDeck).AppendFailureMessage("Shuffled deck order matches original deck order").IsFalse();
    }
}