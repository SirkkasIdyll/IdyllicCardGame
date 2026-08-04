using System;
using System.Linq;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Systems.Player;
using Kolmetoista.Temperance.NCS;
using Kolmetoista.Temperance.Signals;

namespace Kolmetoista.Systems.Rules;

/// <summary>
/// A round starts when all players are FIRST dealt their hands
/// A round ends when only one player has cards left in their hand
/// </summary>
[GlobalClass]
public partial class RoundSystem : NodeSystem
{
    [InjectedDependency] private readonly DeckSystem _deckSystem = null!;
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    [InjectedDependency] private readonly RoomSystem _roomSystem = null!;
    
    /// <summary>
    /// Players participating in the round is anyone with cards still left in hand (means we're excluding late-joins)
    /// </summary>
    private Node<PlayerHandComponent>?[] _playersInRound = new Node<PlayerHandComponent>?[RoomSystem.MaxPlayers];
    
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
    
    /// <summary>
    /// Starts the next round using the current players in the room
    /// </summary>
    /// <returns></returns>
    public bool TryStartRound()
    {
        // If we don't have enough players in the room, the round fails to start
        var playersInRoom = _roomSystem.GetPlayersInRoom();
        if (playersInRoom.Count(x => x!= null) < 2)
        {
            var failedToStartRoundSignal = new FailedToStartRoundSignal();
            _signalBus.EmitFailedToStartRoundSignal(ref failedToStartRoundSignal);
            return false;
        }
        
        // If there are more than two players left with cards, we should also fail to start the round
        // TODO: Add check here
        
        
        // New round can begin, all players in the room are players for the next round
        _playersInRound = playersInRoom;

        // Losing player index is communicated because they're the dealer,
        // and they deal out cards in clockwise order
        var losingPlayerIndex = _playersInRound.IndexOf(_roundLoser) != -1 ? _playersInRound.IndexOf(_roundLoser) : 0;
        _roundLoser = null;
        _deckSystem.DealCards(_playersInRound, losingPlayerIndex);
        
        // Starting player index is communicated so that their turn begins
        var startingPlayerIndex = GetStartingPlayerIndex(_playersInRound, _roundWinner);
        _roundWinner = null;
        var signal = new RoundStartSignal
        {
            StartingPlayerIndex = startingPlayerIndex
        };
        _signalBus.EmitRoundStartSignal(ref signal);

        return true;
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
    private int GetStartingPlayerIndex(Node<PlayerHandComponent>?[] playersInRound, Node<PlayerHandComponent>? roundWinner)
    {
        var startingPlayerIndex = -1;
        
        // Round winner starts
        if (roundWinner != null)
            startingPlayerIndex = playersInRound.IndexOf(roundWinner.Value);
        
        // If no round winner, look for who has the three of spades
        if (startingPlayerIndex == -1)
        {
            foreach (var player in playersInRound)
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
                        startingPlayerIndex = playersInRound.IndexOf(player);
                }
            }
        }
        
        // If no one has the three of spades, just give it to someone man
        if (startingPlayerIndex == -1)
            startingPlayerIndex = 1;

        return startingPlayerIndex;
    }
}

public class FailedToStartRoundSignal : UserSignalArgs;

public class RoundStartSignal : UserSignalArgs
{
    public int StartingPlayerIndex = -1;
}

public class RoundEndSignal : UserSignalArgs;