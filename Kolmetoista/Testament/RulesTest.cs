using System.Collections.Generic;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Systems.Rules;
using Kolmetoista.Temperance.NCS;

namespace Kolmetoista.Testament;

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
    public void RecognizeSingleHandValidTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        // Pull a single card from the deck
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        // Check that a single card is recognized as a Single
        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Single).IsTrue();
        
        // Check that two cards are not recognized as a Single
        playedCards.Add(deck[1]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsFalse();
        AssertObject(handType).IsNull();
    }
    
    /// <summary>
    /// Recognize two cards of the same rank as a pair
    /// Recognize three cards of the same rank as not a pair
    /// </summary>
    [TestCase]
    public void RecognizePairHandValidTest()
    {
        AssertBool(_nodeSystemManager.TryGetNodeSystem<DeckSystem>(out var deckSystem)).IsTrue();
        if (deckSystem is null)
            return;
        
        // Pull two same rank cards from the deck
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        // Check that two cards of the same rank are recognized as a Pair
        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Pair).IsTrue();
        
        // Check that three cards of the same rank are not recognized as a Pair
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
        
        // Pull three cards of the same rank from the deck
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        playedCards.Add(deck[26]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        // Check that three cards of the same rank are recognized as Triples
        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Triples).IsTrue();
        
        // Check that four cards of the same rank are not recognized as Triples
        playedCards.Add(deck[39]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Pair).IsFalse();
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
        
        // Pull four cards of the same rank from the deck
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[0]);
        playedCards.Add(deck[13]);
        playedCards.Add(deck[26]);
        playedCards.Add(deck[39]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        // Check that four cards of the same rank are recognized as Quads
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
        
        // Pull the first three cards from the deck in an unordered fashion
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[2]);
        playedCards.Add(deck[0]);
        playedCards.Add(deck[1]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;
        
        // Check that it is recognized as a sequence
        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that four cards in sequence are recognized as a sequence
        playedCards.Add(deck[3]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that five cards in sequence are recognized as a sequence
        playedCards.Add(deck[4]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that six cards in sequence are recognized as a sequence
        playedCards.Add(deck[5]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that seven cards in sequence are recognized as a sequence
        playedCards.Add(deck[6]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that eight cards in sequence are recognized as a sequence
        playedCards.Add(deck[7]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that nine cards in sequence are recognized as a sequence
        playedCards.Add(deck[8]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that ten cards in sequence are recognized as a sequence
        playedCards.Add(deck[9]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that eleven cards in sequence are recognized as a sequence
        playedCards.Add(deck[10]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that twelve cards in sequence are recognized as a sequence
        playedCards.Add(deck[11]);
        AssertBool(rulesSystem.IsValidHandType(playedCards, out handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.Sequence).IsTrue();
        
        // Check that thirteen cards in sequence are recognized as a sequence
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
        
        // Pull two sets of sequences that are the same rank
        deckSystem.CreateNewDeck(out var deck);
        List<Node<CardComponent>> playedCards = new();
        playedCards.Add(deck[2]);
        playedCards.Add(deck[0]);
        playedCards.Add(deck[1]);
        playedCards.Add(deck[15]);
        playedCards.Add(deck[14]);
        playedCards.Add(deck[13]);
        
        AssertBool(_nodeSystemManager.TryGetNodeSystem<PlayableHandSystem>(out var rulesSystem)).IsTrue();
        if (rulesSystem is null)
            return;

        // Check that two sets of sequences are recognized as a paired sequence
        AssertBool(rulesSystem.IsValidHandType(playedCards, out var handType)).IsTrue();
        AssertObject(handType).IsNotNull();
        AssertBool(handType == HandType.PairedSequence).IsTrue();
        
        // Check that two sets of different sequences are not recognized as a paired sequence
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