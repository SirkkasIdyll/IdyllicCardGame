using System.Collections.Generic;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Systems.Rules;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Testament;

using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite][RequireGodotRuntime]
public class RulesTest
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

    /// <summary>
    /// Recognize single card as single
    /// Recognize two cards as nothing
    /// </summary>
    [TestCase]
    public void SingleHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Single).IsTrue();
        
        playedCards.Add(deck[1]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsFalse();
        AssertObject(handType).IsNull();
    }
    
    /// <summary>
    /// Recognize two cards of the same rank as a pair
    /// Recognize three cards of the same rank as not a pair
    /// </summary>
    [TestCase]
    public void PairHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Pair).IsTrue();
        
        playedCards.Add(deck[26]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Pair).IsFalse();
    }
    
    /// <summary>
    /// Recognize three cards of the same rank as triples
    /// </summary>
    [TestCase]
    public void TripleHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        playedCards.Add(deck[26]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Triples).IsTrue();
    }
    
    /// <summary>
    /// Recognize four cards of the same rank as quads
    /// </summary>
    [TestCase]
    public void QuadHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        playedCards.Add(deck[26]);
        playedCards.Add(deck[39]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Quads).IsTrue();
    }
    
    /// <summary>
    /// Recognize three cards or more in a row as a sequence (order of cards given does not matter)
    /// </summary>
    [TestCase]
    public void SequenceHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[2]);
        playedCards.Add(deck[0]);
        playedCards.Add(deck[1]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[3]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[4]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[5]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[6]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[7]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[8]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[9]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[10]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[11]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        playedCards.Add(deck[12]);
        
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
    }
    
    /// <summary>
    /// Recognize that a pair of a sequence is indeed, a pair of a sequence
    /// Recognize that two different sequences does not make a pair of a sequence
    /// </summary>
    [TestCase]
    public void PairedSequenceHandTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[2]);
        playedCards.Add(deck[0]);
        playedCards.Add(deck[1]);
        playedCards.Add(deck[15]);
        playedCards.Add(deck[13]);
        playedCards.Add(deck[14]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<RulesSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.PairedSequence).IsTrue();
        
        playedCards.RemoveAt(3);
        playedCards.RemoveAt(3);
        playedCards.RemoveAt(3);
        playedCards.Add(deck[14]);
        playedCards.Add(deck[15]);
        playedCards.Add(deck[16]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsFalse();
        AssertObject(handType).IsNull();
        AssertBool(handType == HandType.PairedSequence).IsFalse();
    }
}