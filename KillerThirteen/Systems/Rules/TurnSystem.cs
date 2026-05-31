using System.Collections.Generic;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;
using KillerThirteen.Temperance.Signals;

namespace KillerThirteen.Systems.Rules;

public partial class TurnSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    [InjectedDependency] private readonly DeckSystem _deckSystem = null!;
    
    /// <summary>
    /// List of players in the game, doesn't necessarily mean they're in the round
    /// All players are added to the round at the start of each round
    /// </summary>
    private List<Node<HandComponent>> _players = [];
    
    /// <summary>
    /// List of players still playing in the hand and haven't passed yet
    /// Order should be clock-wise
    /// </summary>
    private List<Node<HandComponent>> _orderedPlayersStillIn = new();
    
    // Current player is not in control of hand until they actually play something
    // They can choose to play or pass
    private Node<HandComponent>? _currentPlayerTurn;

    // The player in control of hand is still in the round
    // They get to play whatever they want if everyone else passes the turn back to them
    private Node<HandComponent>? _inControlOfHand;
    
    // Round winner is the player that finishes first, they start first next round
    private Node<HandComponent>? _roundWinner;
    
    // Round loser is the last player with cards in hand, they have to shuffle the deck and deal cards
    private Node<HandComponent>? _roundLoser;
    
    /// <summary>
    /// A player should be able to join a game at any time but not necessarily be in the hand
    /// They can be dealt in next round
    /// </summary>
    /// <param name="player"></param>
    public void JoinGame(Node<HandComponent> player)
    {
        _players.Add(player);
    }

    /// <summary>
    /// A player should be able to leave a game at any time
    /// If it's their turn, they should pass their turn
    /// If it's not their turn, just remove them from the turn queue
    /// </summary>
    /// <param name="player"></param>
    public void LeaveGame(Node<HandComponent> player)
    {
        _players.Remove(player);

        if (_currentPlayerTurn != null && _currentPlayerTurn.Value.Equals(player))
        {
            PassTurn(player);
            return;
        }
        
        var index = _orderedPlayersStillIn.IndexOf(player);
        _currentPlayerTurn = _orderedPlayersStillIn[(index + 1) % _orderedPlayersStillIn.Count];
        _orderedPlayersStillIn.Remove(player);
    }

    /// <summary>
    /// If you pass your turn, you're out of the current round and the turn is passed to the next player in line
    /// </summary>
    /// <param name="player"></param>
    public void PassTurn(Node<HandComponent> player)
    {
        if (_currentPlayerTurn == null)
            return;

        if (!_currentPlayerTurn.Value.Equals(player))
            return;
        
        var currentPlayerIndex = _orderedPlayersStillIn.IndexOf(_currentPlayerTurn.Value);
        _currentPlayerTurn = _orderedPlayersStillIn[(currentPlayerIndex + 1) % _orderedPlayersStillIn.Count];
        _orderedPlayersStillIn.Remove(player);
    }
    
    /// <summary>
    /// Deal cards, determine player order, and set current player's turn
    /// </summary>
    public void StartRound()
    {
        // Takes two to tango
        // Threesome to be somethin...
        // But a fourway to bust your doorway
        if (_players.Count < 2)
            return;
        
        var dealerIndex = _roundLoser != null ? _players.IndexOf(_roundLoser.Value) : 0;
        _deckSystem.DealCards(_players, dealerIndex);
        DeterminePlayerOrder();

        var signal = new RoundStartSignal();
        _signalBus.EmitRoundStartSignal(ref signal);
        _currentPlayerTurn = _orderedPlayersStillIn[0];
    }
    
    // <summary>
    // Starter player in the order is determined by
    // 1. If there was a round winner, they start first and have freedom to play whatever
    // 2. Whoever has the three of spades gets to start
    // Player order goes clock-wise beginning with the starting player
    // </summary>
    private void DeterminePlayerOrder()
    {
        _orderedPlayersStillIn.Clear();
        var startingPlayerIndex = 0;
        if (_roundWinner != null)
            startingPlayerIndex = _players.IndexOf(_roundWinner.Value);
        
        if (_roundWinner == null)
        {
            // I don't really care if it's inefficient and loops through the rest, it shouldn't take that long
            foreach (var player in _players)
            {
                foreach (var card in player.Comp.Cards)
                {
                    if (card.Comp.Rank == CardRank.Three && card.Comp.Suit == CardSuit.Spades)
                        startingPlayerIndex = _players.IndexOf(player);
                }
            }
        }

        // Ex: If winning player is the 4th player (index 3)
        // i = 0; (0 + 3) % 4 = 3
        // i = 1; (1 + 3) % 4 = 0
        // i = 2; (2 + 3) % 4 = 1
        // i = 3; (3 + 3) % 4 = 2
        for (int i = 0; i < _players.Count; i++)
            _orderedPlayersStillIn.Add(_players[(i + startingPlayerIndex) % _players.Count]);
    }
}

public class RoundStartSignal : UserSignalArgs;