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
    private readonly List<Node<HandComponent>> _lobbyPlayers = [];
    
    // Current player is not in control of hand until they actually play something
    // They can choose to play or pass
    private Node<HandComponent>? _currentPlayerTurn;

    /// <summary>
    /// Players still participating in the current hand who haven't passed yet
    /// </summary>
    private readonly List<Node<HandComponent>> _handPlayers = new();

    /// <summary>
    /// Players participating in the round is anyone with cards still left in hand (means we're excluding late-joins)
    /// </summary>
    private readonly List<Node<HandComponent>> _roundPlayers = new();
    
    // Round winner is the player that finishes first, they start first next round
    private Node<HandComponent>? _roundWinner;
    
    // Round loser is the last player with cards in hand, they have to shuffle the deck and deal cards
    private Node<HandComponent>? _roundLoser;

    // The player in control of hand is still in the round
    // They get to play whatever they want if everyone else passes the turn back to them
    private Node<HandComponent>? _inControlOfHand;

    public override void _Ready()
    {
        base._Ready();

        _signalBus.HandPlayedSignal += OnHandPlayed;
    }

    /// <summary>
    /// When a hand is played, if the player is out of cards, pull them out of the round
    /// Otherwise they are now in control of the hand and the turn passes to the next player
    /// If there is only one player left after the hand is played,
    /// the last player is the round loser and the round should end
    /// </summary>
    private void OnHandPlayed(Node<HandComponent> player, ref HandPlayedSignal args)
    {
        NextHandPlayerTurn();
        
        if (player.Comp.Cards.Count == 0)
        {
            // Round winner is player if no current round winner
            _roundWinner ??= player;
            
            // Player is removed from the round since they have no cards left to play
            _roundPlayers.Remove(player);

            // If the amount of players left is one or less, the round ends
            if (_roundPlayers.Count <= 1)
            {
                EndRound();
                return;
            }
            
            // If there are still more players left, the next player in line is in control
            _inControlOfHand = _currentPlayerTurn;
            return;
        }

        // The player who just played the hand still has more cards, so they're in control of the hand
        _inControlOfHand = player;
    }

    /// <summary>
    /// A player should be able to join a game at any time but not necessarily be in the hand
    /// They can be dealt in next round
    /// </summary>
    /// <param name="player"></param>
    public void JoinGame(Node<HandComponent> player)
    {
        _lobbyPlayers.Add(player);
    }

    /// <summary>
    /// A player should be able to leave a game at any time
    /// If it's their turn, they should pass their turn
    /// If it's not their turn, just remove them from the turn queue
    /// </summary>
    /// <param name="player"></param>
    public void LeaveGame(Node<HandComponent> player)
    {
        _lobbyPlayers.Remove(player);

        if (_currentPlayerTurn != null && _currentPlayerTurn.Value.Equals(player))
        {
            PassTurn(player);
            return;
        }
        
        NextHandPlayerTurn();
        _handPlayers.Remove(player);
        _roundPlayers.Remove(player);
    }

    /// <summary>
    /// If you pass your turn, you're out of the current round and the turn is passed to the next player in line
    /// If there is only one player left, the hand restarts
    /// </summary>
    /// <param name="player"></param>
    public void PassTurn(Node<HandComponent> player)
    {
        if (_currentPlayerTurn == null)
            return;

        if (!_currentPlayerTurn.Value.Equals(player))
            return;
        
        NextHandPlayerTurn();
        _handPlayers.Remove(player);
    }
    
    /// <summary>
    /// Deal cards, determine player order, and set current player's turn
    /// </summary>
    public void StartRound()
    {
        // Takes two to tango
        // Threesome to be somethin...
        // But a fourway to bust your doorway
        if (_lobbyPlayers.Count < 2)
            return;

        var dealerIndex = _roundLoser != null ? _lobbyPlayers.IndexOf(_roundLoser.Value) : 0;
        _deckSystem.DealCards(_lobbyPlayers, dealerIndex);
        
        DeterminePlayerOrder();
        var signal = new RoundStartSignal();
        _signalBus.EmitRoundStartSignal(ref signal);
        
        _roundWinner = null;
        _roundLoser = null;
    }

    private void EndRound()
    {
        _roundLoser = _currentPlayerTurn;
        
        var signal = new RoundEndSignal();
        _signalBus.EmitRoundEndSignal(ref signal);
    }
    
    // <summary>
    // Starter player in the order is determined by
    // 1. If there was a round winner, they start first and have freedom to play whatever
    // 2. Whoever has the three of spades gets to start
    // Player order goes clock-wise beginning with the starting player
    // </summary>
    private void DeterminePlayerOrder()
    {
        _handPlayers.Clear();
        var startingPlayerIndex = 0;
        if (_roundWinner != null)
            startingPlayerIndex = _roundPlayers.IndexOf(_roundWinner.Value);
        
        if (_roundWinner == null)
        {
            // I don't really care if it's inefficient and loops through the rest, it shouldn't take that long
            foreach (var player in _roundPlayers)
            {
                foreach (var card in player.Comp.Cards)
                {
                    if (card.Comp.Rank == CardRank.Three && card.Comp.Suit == CardSuit.Spades)
                        startingPlayerIndex = _roundPlayers.IndexOf(player);
                }
            }
        }

        // Ex: If winning player is the 4th player (index 3)
        // i = 0; (0 + 3) % 4 = 3
        // i = 1; (1 + 3) % 4 = 0
        // i = 2; (2 + 3) % 4 = 1
        // i = 3; (3 + 3) % 4 = 2
        for (int i = 0; i < _roundPlayers.Count; i++)
            _handPlayers.Add(_roundPlayers[(i + startingPlayerIndex) % _roundPlayers.Count]);
        
        _currentPlayerTurn = _handPlayers[0];
    }

    /// <summary>
    /// Goes to the next player's turn who's still participating in the hand
    /// </summary>
    private void NextHandPlayerTurn()
    {
        if (_currentPlayerTurn == null)
            return;
        
        var currentPlayerIndex = _handPlayers.IndexOf(_currentPlayerTurn.Value);
        _currentPlayerTurn = _handPlayers[(currentPlayerIndex + 1) % _handPlayers.Count];
    }
}

public class RoundStartSignal : UserSignalArgs;
public class RoundEndSignal : UserSignalArgs;