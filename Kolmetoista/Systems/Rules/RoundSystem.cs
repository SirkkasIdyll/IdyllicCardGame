using System.Collections.Generic;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Systems.Player;
using Kolmetoista.Temperance.NCS;
using Kolmetoista.Temperance.Signals;

namespace Kolmetoista.Systems.Rules;

/// <summary>
/// A round starts when all players are dealt their hands
/// A round ends when only one player has cards left in their hand
/// </summary>
[GlobalClass]
public partial class RoundSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    
    /// <summary>
    /// Players participating in the round is anyone with cards still left in hand (means we're excluding late-joins)
    /// </summary>
    private readonly Node<PlayerHandComponent>?[] _playersInRound = new Node<PlayerHandComponent>?[4];
    
    // Round winner is the player that finishes first
    // They go first at the start of the next round
    private Node<PlayerHandComponent>? _roundWinner;
    
    // Round loser is the last player with cards in hand
    // They have to shuffle the deck and deal cards out to other players
    private Node<PlayerHandComponent>? _roundLoser;
    
    public override void _Ready()
    {
        base._Ready();

        _signalBus.LeaveRoomSignal += OnLeaveRoom;
    }

    private void OnLeaveRoom(Node<PlayerHandComponent> player, ref LeaveRoomSignal args)
    {
        _playersInRound.Remove(player);
    }

    private void StartRound()
    {
        var signal = new RoundStartSignal        
        {
            RoundWinner = _roundWinner,
            RoundLoser = _roundLoser
        };
        _signalBus.EmitRoundStartSignal(ref signal);
        
        _roundWinner = null;
        _roundLoser = null;
    }

    private void EndRound()
    {
        var signal = new RoundEndSignal();
        _signalBus.EmitRoundEndSignal(ref signal);
    }
    
    // <summary>
    // Starter player in the order is determined by
    // 1. If there was a round winner, they start first and have freedom to play whatever
    // 2. Whoever has the three of spades gets to start
    // Player order goes clock-wise beginning with the starting player
    // </summary>
    public void DeterminePlayerOrder()
    {
        // _handPlayers.Clear();
        var startingPlayerIndex = 0;
        if (_roundWinner != null)
            startingPlayerIndex = _playersInRound.IndexOf(_roundWinner.Value);
        
        if (_roundWinner == null)
        {
            // I don't really care if it's inefficient and loops through the rest, it shouldn't take that long
            foreach (var player in _playersInRound)
            {
                foreach (var card in player.Comp.Cards)
                {
                    if (card.Comp.Rank == CardRank.Three && card.Comp.Suit == CardSuit.Spades)
                        startingPlayerIndex = _playersInRound.IndexOf(player);
                }
            }
        }

        // Ex: If winning player is the 4th player (index 3)
        // i = 0; (0 + 3) % 4 = 3
        // i = 1; (1 + 3) % 4 = 0
        // i = 2; (2 + 3) % 4 = 1
        // i = 3; (3 + 3) % 4 = 2
        // for (int i = 0; i < _playersInRound.Count; i++)
        //     _handPlayers.Add(_playersInRound[(i + startingPlayerIndex) % _playersInRound.Count]);
        //
        // _currentPlayerTurn = _handPlayers[0];
    }
}

public class RoundStartSignal : UserSignalArgs
{
    public Node<PlayerHandComponent>? RoundWinner;
    public Node<PlayerHandComponent>? RoundLoser;
}
public class RoundEndSignal : UserSignalArgs;