using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Rules;

[GlobalClass]
public partial class RulesSystem : NodeSystem
{
    private readonly List<Node<CardComponent>> _lastSortedHandPlayed = [];
    private HandType _currentRoundHandType;

    public bool CanPlayHand(List<Node<CardComponent>> cards)
    {
        if (!IsValidHandType(cards, out var handType))
            return false;
        
        // if (handType != _currentRoundHandType)
        //     
        
        return false;
    }

    public bool IsValidHandType(List<Node<CardComponent>> cards, [NotNullWhen(true)] out HandType? handType)
    {
        handType = null;
        var sortedHand = SortHand(cards);

        var handString = "Played: ";
        foreach (var card in sortedHand)
            handString += card.Comp.Rank + " of " + card.Comp.Suit + ", ";
        GD.Print(handString.Substring(0, handString.Length - 2));
        
        if (cards.Count == 1)
        {
            handType = HandType.Single;
            return true;
        }

        if (cards.Count == 2)
        {
            if (cards[0].Comp.Rank == cards[1].Comp.Rank)
            {
                handType = HandType.Pair;
                return true;
            }
        }

        if (cards.Count == 3)
        {
            if (cards[0].Comp.Rank == cards[1].Comp.Rank &&
                cards[1].Comp.Rank == cards[2].Comp.Rank)
            {
                handType = HandType.Triples;
                return true;
            }
        }

        if (cards.Count == 4)
        {
            if (cards[0].Comp.Rank == cards[1].Comp.Rank &&
                cards[1].Comp.Rank == cards[2].Comp.Rank &&
                cards[2].Comp.Rank == cards[3].Comp.Rank)
            {
                handType = HandType.Quads;
                return true;
            }
        }

        if (cards.Count >= 3)
        {
            // Check for sequence
            if (sortedHand.Zip(sortedHand.Skip(1), (node1, node2) => node1.Comp.Rank + 1 == node2.Comp.Rank).All(x => x))
            {
                handType = HandType.Sequence;
                return true;
            }
        }

        // Must be an even amount of cards and enough cards to validly make up a paired sequence
        if (cards.Count >= 6 && cards.Count % 2 == 0)
        {
            var secondHalf = cards.Count / 2;
            for (int i = 0; i < cards.Count / 2 - 1; i++)
            {
                if (sortedHand[i].Comp.Rank + 1 == sortedHand[i + 1].Comp.Rank &&
                    sortedHand[i].Comp.Rank == sortedHand[i + secondHalf].Comp.Rank)
                    continue;

                return false;
            }

            handType = HandType.PairedSequence;
            return true;
        }

        return false;
    }
    
    public void GetCurrentRoundHandType(out HandType handType)
    {
        handType = _currentRoundHandType;
    }

    private List<Node<CardComponent>> SortHand(List<Node<CardComponent>> cards)
    {
        return cards.OrderBy(x => x.Comp.Suit).ThenBy(x => x.Comp.Rank).ToList();
    }
}

public enum HandType
{
    Single,
    Pair,
    Triples,
    Quads,
    Sequence,
    PairedSequence
}