using System;
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
    private readonly Node<PlayerHandComponent>?[] _playersInRound = new Node<PlayerHandComponent>?[RoomSystem.MaxPlayers];
    
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
        var index = _playersInRound.IndexOf(player);
        
        if (index != -1)
            _playersInRound[index] = null;
    }

    /// <summary>
    /// Return the players who still have cards left to play
    /// </summary>
    public Node<PlayerHandComponent>?[] GetPlayersInRound()
    {
        return _playersInRound;
    }

    public void StartRound()
    {
        var signal = new RoundStartSignal
        {
            StartingPlayerIndex = GetStartingPlayerIndex(_roundWinner)
        };
        _signalBus.EmitRoundStartSignal(ref signal);
        
        _roundWinner = null;
        _roundLoser = null;
    }

    public void EndRound()
    {
        var signal = new RoundEndSignal();
        _signalBus.EmitRoundEndSignal(ref signal);
    }
    
    // <summary>
    // Starting player order:
    // 1. If there was a round winner, they start first and have freedom to play whatever
    // 2. Else, whoever has the three of spades gets to start and needs to play the three of spades
    // 3. If we're playing with less than 4 people, and no one has the three, just give it to them
    // Then player order goes clockwise
    // </summary>
    private int GetStartingPlayerIndex(Node<PlayerHandComponent>? roundWinner)
    {
        var startingPlayerIndex = -1;
        
        // Round winner starts
        if (roundWinner != null)
            startingPlayerIndex = _playersInRound.IndexOf(roundWinner.Value);
        
        // If no round winner, look for who has the three of spades
        if (startingPlayerIndex == -1)
        {
            foreach (var player in _playersInRound)
            {
                if (player == null)
                    continue;

                if (startingPlayerIndex != -1)
                    break;
                
                foreach (var card in player.Value.Comp.Cards)
                {
                    if (startingPlayerIndex != -1)
                        break;
                    
                    if (card.Comp is { Rank: CardRank.Three, Suit: CardSuit.Spades })
                        startingPlayerIndex = _playersInRound.IndexOf(player);
                }
            }
        }
        
        // If no one has the three of spades, just give it to someone man
        if (startingPlayerIndex == -1)
            startingPlayerIndex = 1;

        return startingPlayerIndex;
    }
}

public class RoundStartSignal : UserSignalArgs
{
    public int StartingPlayerIndex = -1;
}

public class RoundEndSignal : UserSignalArgs;