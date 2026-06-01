using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;
using KillerThirteen.Temperance.Signals;

namespace KillerThirteen.Systems.Rules;

/// <summary>
/// Responsible for checking valid hands and playable hands
/// </summary>
[GlobalClass]
public partial class HandSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    
    /// <summary>
    /// If something logically needs to work when sorted, it should be sorted immediately before you need it to be sorted
    /// Rather than being assumed to be sorted
    /// </summary>
    private readonly List<Node<CardComponent>> _lastHandPlayed = [];
    private HandType _currentHandType;

    public override void _Ready()
    {
        base._Ready();

        _signalBus.RoundStartSignal += OnRoundStart;
    }

    private void OnRoundStart(ref RoundStartSignal args)
    {
        _lastHandPlayed.Clear();
    }

    /// <summary>
    /// Check if a hand is valid given the current round context
    /// </summary>
    /// <param name="cards">Unsorted cards the player wants to play</param>
    /// <returns>True if you beat the last hand, false if you don't</returns>
    public bool CanPlayHand(List<Node<CardComponent>> cards)
    {
        // What are you even doing if you're not playing a valid hand
        if (!IsValidHandType(cards, out var handType))
            return false;

        if (_lastHandPlayed.Count == 0)
            return true;

        // The specific scenarios when a two loses
        // Also known as "bombs"
        // Also the only time you can play a different hand type to respond to a hand
        if (_currentHandType is HandType.Single or HandType.Pair or HandType.Triples
            && _lastHandPlayed[0].Comp.Rank == CardRank.Two)
        {
            switch (_currentHandType)
            {
                // A single two can be beaten by 3+ paired sequence and quads
                case HandType.Single when handType == HandType.PairedSequence && cards.Count >= 6:
                case HandType.Single when handType == HandType.Quads:
                // A pair of twos can be beaten by 4+ paired sequence or quads
                case HandType.Pair when handType == HandType.PairedSequence && cards.Count >= 8:
                case HandType.Pair when handType == HandType.Quads:
                // A triple of twos can be beaten by 5+ paired sequence
                case HandType.Triples when handType == HandType.PairedSequence && cards.Count >= 10:
                    return true;
                default:
                    break;
            }
        }

        // Besides the special scenario above, you gotta match the current round's hand type
        if (handType != _currentHandType)
            return false;
        
        var sortedHand = SortHand(cards);
        var lastSortedHandPlayed = SortHand(_lastHandPlayed);

        if (_currentHandType is HandType.Single or HandType.Pair or HandType.Triples or HandType.Quads)
        {
            // Greater rank always wins
            if (sortedHand[0].Comp.Rank > lastSortedHandPlayed[0].Comp.Rank)
                return true;

            // If you play the same hand, greater suit wins
            // It's impossible to play the same rank with triples or quads so we don't need to check
            if (sortedHand[0].Comp.Rank == lastSortedHandPlayed[0].Comp.Rank)
            {
                if (_currentHandType is HandType.Single
                    && sortedHand[0].Comp.Suit > lastSortedHandPlayed[0].Comp.Suit)
                    return true;

                if (_currentHandType == HandType.Pair
                    && sortedHand[1].Comp.Suit > lastSortedHandPlayed[1].Comp.Suit)
                    return true;
            }
        }

        // You can only play a sequence of the same length as the current round
        if (_currentHandType is HandType.Sequence or HandType.PairedSequence
            && sortedHand.Count == lastSortedHandPlayed.Count)
        {
            // TIL I about the hat operator to get the last index
            // If the last number in the sorted sequence is greater, you win
            if (sortedHand[^1].Comp.Rank > lastSortedHandPlayed[^1].Comp.Rank)
                return true;

            // If the sequence is the same but your suit is better you also win
            if (sortedHand[^1].Comp.Rank == lastSortedHandPlayed[^1].Comp.Rank
                && sortedHand[^1].Comp.Suit > lastSortedHandPlayed[^1].Comp.Suit)
                return true;
        }

        // You didn't have a suit or rank greater than your opponent's :(
        return false;
    }

    /// <summary>
    /// Check if a hand is valid irrelevant to the current round context
    /// </summary>
    /// <param name="cards">Unsorted cards the player wants to play</param>
    /// <param name="handType">The category of hand when hand is valid</param>
    /// <returns>True if valid, false if it's nothing</returns>
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
    
    public void PlayHand(Node<HandComponent> player, List<Node<CardComponent>> selectedCards)
    {
        if (!IsValidHandType(selectedCards, out var handType))
            return;
            
        if (!CanPlayHand(selectedCards))
            return;

        foreach (var card in selectedCards)
            player.Comp.Cards.Remove(card);
        
        _lastHandPlayed.AddRange(selectedCards);
        _currentHandType = handType.Value;

        var signal = new HandPlayedSignal(player);
        _signalBus.EmitHandPlayedSignal(player, ref signal);
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

public class HandPlayedSignal : UserSignalArgs
{
    public Node<HandComponent> Node;

    public HandPlayedSignal(Node<HandComponent> node)
    {
        Node = node;
    }
}