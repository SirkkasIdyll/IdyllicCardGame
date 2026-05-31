using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using KillerThirteen.Systems.Cards;
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
        _nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem);
        AssertObject(deckSystem).IsNotNull();

        if (deckSystem is null)
            return;

        deckSystem.CreateNewDeck(out var deck);
        AssertBool(deck.Count == 52).AppendFailureMessage(
            "Failed to generate a proper deck of fifty two cards."
            ).IsTrue();

        var originalDeck =  new Node[52];
        deck.CopyTo(originalDeck);
        
        deckSystem.ShuffleDeck(out deck);
        var matchesOriginalDeck = true;
        for (var i = deck.Count - 1; i > 0; i--)
        {
            if (!_componentManager.TryGetComponent<CardComponent>(originalDeck[i], out var cardComp1))
                continue;

            if (!_componentManager.TryGetComponent<CardComponent>(deck[i], out var cardComp2))
                continue;
            
            GD.Print(cardComp1.Rank + " of " + cardComp1.Suit + " vs " + cardComp2.Rank + " of " + cardComp2.Suit);

            if (cardComp1.Suit == cardComp2.Suit && cardComp1.Rank == cardComp2.Rank)
                continue;

            matchesOriginalDeck = false;
            break;
        }
        AssertBool(matchesOriginalDeck).AppendFailureMessage("Shuffled deck order matches original deck order").IsFalse();
    }
}