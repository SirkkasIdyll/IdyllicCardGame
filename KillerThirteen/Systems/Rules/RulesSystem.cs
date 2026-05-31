using System.Collections.Generic;
using System.Linq;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Rules;

[GlobalClass]
public partial class RulesSystem : NodeSystem
{
    private HandType _currentRoundHandType;

    public bool IsValidHandType(List<Node<CardComponent>> cardsPlayed, out HandType? handType)
    {
        handType = null;
        var sortedHand = cardsPlayed.OrderBy(x => x.Comp.Rank).ToList();

        if (cardsPlayed.Count == 1)
        {
            handType = HandType.Single;
            return true;
        }

        if (cardsPlayed.Count == 2)
        {
            if (cardsPlayed[0].Comp.Rank == cardsPlayed[1].Comp.Rank)
            {
                handType = HandType.Pair;
                return true;
            }
        }

        if (cardsPlayed.Count == 3)
        {
            if (cardsPlayed[0].Comp.Rank == cardsPlayed[1].Comp.Rank &&
                cardsPlayed[1].Comp.Rank == cardsPlayed[2].Comp.Rank)
            {
                handType = HandType.Triples;
                return true;
            }
        }

        if (cardsPlayed.Count == 4)
        {
            if (cardsPlayed[0].Comp.Rank == cardsPlayed[1].Comp.Rank &&
                cardsPlayed[1].Comp.Rank == cardsPlayed[2].Comp.Rank &&
                cardsPlayed[2].Comp.Rank == cardsPlayed[3].Comp.Rank)
            {
                handType = HandType.Quads;
                return true;
            }
        }

        if (cardsPlayed.Count >= 3)
        {
            // Check for sequence
            if (sortedHand.Zip(sortedHand.Skip(1), (node1, node2) => node1.Comp.Rank + 1 == node2.Comp.Rank).All(x => x))
            {
                handType = HandType.Sequence;
                return true;
            }
        }

        // Must be an even amount of cards and enough cards to validly make up a paired sequence
        if (cardsPlayed.Count >= 6 && cardsPlayed.Count % 2 == 0)
        {
            var (checkPair, checkSequence) = (true, false);
            for (int i = 0; i < sortedHand.Count - 1; i++)
            {
                if (checkPair)
                {
                    if (sortedHand[i].Comp.Rank == sortedHand[i + 1].Comp.Rank)
                    {
                        (checkPair, checkSequence) = (checkSequence, checkPair);
                        continue;
                    }

                    return false;
                }

                if (checkSequence)
                {
                    if (sortedHand[i].Comp.Rank + 1 == sortedHand[i + 1].Comp.Rank)
                    {
                        (checkPair, checkSequence) = (checkSequence, checkPair);
                        continue;
                    }

                    return false;
                }
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