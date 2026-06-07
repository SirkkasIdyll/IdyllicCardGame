using System;
using System.Collections.Generic;
using Godot;
using KillerThirteen.Systems.Player;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Cards;

[GlobalClass]
public partial class DeckSystem : NodeSystem
{
    [InjectedDependency] private readonly ComponentManager _componentManager = null!;
    [InjectedDependency] private readonly NodeManager _nodeManager = null!;
    // [InjectedDependency] private readonly NodeSystemManager _nodeSystemManager = null!;
    
    private static readonly Random RNG = new Random();
    private readonly List<Node<CardComponent>> _deck = new();

    public override void _Ready()
    {
        base._Ready();
        
        CreateNewDeck(out _);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        ClearDeck();
    }

    private void ClearDeck()
    {
        foreach (var node in _deck)
            node.Owner.QueueFree();
        
        _deck.Clear();
    }

    /// <summary>
    /// Adds one of each suit and rank to the deck (it also spawns the cards into the world)
    /// </summary>
    public void CreateNewDeck(out List<Node<CardComponent>> deck)
    {
        ClearDeck();
        
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
                _deck.Add((node3D, cardComponent));
            }
        }

        deck = _deck;
    }

    /// <summary>
    /// Add cards to each player's hands until each player has a total of 13 cards
    /// </summary>
    /// <param name="players">The players participating in the round that need cards</param>
    /// <param name="dealerIndex">The index of the dealer so we can deal in clock-wise fashion</param>
    public void DealCards(List<Node<PlayerHandComponent>> players, int dealerIndex)
    {
        ShuffleDeck(out var deck);
        
        var cardCountToDeal = players.Count * 13;
        for (int i = 0; i < cardCountToDeal - 1; i++)
        {
            // We want to deal to the player clock-wise to the dealer and continue from there
            // i = 0; (0 + 0 + 1) % 4 = 1
            // i = 1; (1 + 0 + 1) % 4 = 2
            // i = 2; (2 + 0 + 1) % 4 = 3
            // i = 3; (3 + 0 + 1) % 4 = 0
            // i = 4; (4 + 0 + 1) % 4 = 1...
            var index = (i + dealerIndex + 1) % players.Count;
            players[index].Comp.Cards.Add(deck[i]);
        }
    }

    public void GetDeck(out List<Node<CardComponent>> deck)
    {
        deck = _deck;
    }

    /// <summary>
    /// Simplified Fisher-Yates shuffle
    /// </summary>
    public void ShuffleDeck(out List<Node<CardComponent>> deck)
    {
        for (var n = _deck.Count - 1; n > 1; n--)
        {
            var k = RNG.Next(n);
            (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
        }

        deck = _deck;
    }
}