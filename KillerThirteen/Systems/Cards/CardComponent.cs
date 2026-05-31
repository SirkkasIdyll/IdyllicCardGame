using Godot;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Cards;

[GlobalClass]
public partial class CardComponent : Component
{
    /// <summary>
    /// Spades is beat by Clubs is beat by Diamonds is beat by Hearts
    /// </summary>
    [Export]
    public CardSuit Suit;
    
    /// <summary>
    /// Regular card rank except two is the highest value
    /// </summary>
    [Export]
    public CardRank Rank;
}


public enum CardSuit
{
    Spades,
    Clubs,
    Diamonds,
    Hearts
}

public enum CardRank
{
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace,
    Two
}