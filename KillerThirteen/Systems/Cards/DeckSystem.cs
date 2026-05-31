using System;
using System.Collections.Generic;
using Godot;
using KillerThirteen.Temperance.NCS;
using KillerThirteen.Temperance.Signals;

namespace KillerThirteen.Systems.Cards;

[GlobalClass]
public partial class DeckSystem : NodeSystem
{
    [InjectedDependency] private readonly ComponentManager _componentManager = null!;
    [InjectedDependency] private readonly NodeManager _nodeManager = null!;
    // [InjectedDependency] private readonly NodeSystemManager _nodeSystemManager = null!;
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    
    private static readonly Random RNG = new Random();
    private readonly List<Node> _deck = new();
    
    /// <summary>
    /// Adds one of each suit and rank to the deck (it also spawns the cards into the world)
    /// </summary>
    public void CreateNewDeck(out List<Node> deck)
    {
        _deck.Clear();
        
        foreach (var suit in Enum.GetValues<CardSuit>())
        {
            foreach (var rank in Enum.GetValues<CardRank>())
            {
                if (!_nodeManager.TrySpawnNode("Card", out var node3D))
                    continue;

                if (!_componentManager.TryGetComponent<CardComponent>(node3D, out var cardComponent))
                    continue;

                cardComponent.Suit = suit;
                cardComponent.Rank = rank;
                _deck.Add(node3D);
            }
        }

        deck = _deck;
    }

    /// <summary>
    /// Simplified Fisher-Yates shuffle
    /// </summary>
    public void ShuffleDeck(out List<Node> deck)
    {
        for (var n = _deck.Count - 1; n > 1; n--)
        {
            var k = RNG.Next(n);
            (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
        }

        deck = _deck;
    }
}