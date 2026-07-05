using System.Collections.Generic;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Systems.Player;
using Kolmetoista.Temperance.NCS;
using Kolmetoista.Temperance.Signals;

namespace Kolmetoista.Systems.Rules;

/// <summary>
/// The "current hand" starts when a player first plays a hand type
/// The "current hand" ends when all other players pass
/// The next "current hand" is started by the player who was able to last play cards
/// </summary>
[GlobalClass]
public partial class CurrentHandSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;
    [InjectedDependency] private readonly PlayableHandSystem _playableHandSystem = null!;
    
    /// <summary>
    /// Players still participating in the current hand who haven't passed yet
    /// </summary>
    private readonly Node<PlayerHandComponent>?[] _playersInHand = new Node<PlayerHandComponent>?[4];
    
    // The player in control of hand is still in the round
    // They get to play whatever they want if everyone else passes the turn back to them
    private Node<PlayerHandComponent>? _inControlOfHand;
    
    // Current player is not in control of hand until they actually play something
    // They can choose to play or pass
    private Node<PlayerHandComponent>? _currentPlayerTurn;
    
    private readonly List<Node<CardComponent>> _lastHandPlayed = [];
    
    private HandType _currentHandType;

    public override void _Ready()
    {
        base._Ready();

        _signalBus.LeaveRoomSignal += OnLeftRoom;
    }

    private void OnLeftRoom(Node<PlayerHandComponent> player, ref LeaveRoomSignal args)
    {
        if (_currentPlayerTurn != null && _currentPlayerTurn.Value.Equals(player))
        {
            PassTurn(player);
            return;
        }
        
        _playersInHand.Remove(player);
    }

    /// <summary>
    /// If you pass your turn, you're out of the current round and the turn is passed to the next player in line
    /// If there is only one player left, the hand restarts
    /// </summary>
    /// <param name="player"></param>
    private void PassTurn(Node<PlayerHandComponent> player)
    {
        if (_currentPlayerTurn == null)
            return;

        if (!_currentPlayerTurn.Value.Equals(player))
            return;
        
        // go to next player's turn
    }
    
    private bool TryPlayHand(Node<PlayerHandComponent> player, List<Node<CardComponent>> selectedCards)
    {
        if (!_playableHandSystem.IsValidHandType(selectedCards, out var handType))
            return false;
            
        if (!_playableHandSystem.CanPlayHand(selectedCards, _lastHandPlayed, _currentHandType))
            return false;

        foreach (var card in selectedCards)
            player.Comp.Cards.Remove(card);
        
        _lastHandPlayed.AddRange(selectedCards);
        _currentHandType = handType.Value;
        
        
        
        
        var signal = new HandPlayedSignal(player);
        _signalBus.EmitHandPlayedSignal(player, ref signal);
    }
    
    //     /// <summary>
    // /// When a hand is played, if the player is out of cards, pull them out of the round
    // /// Otherwise they are now in control of the hand and the turn passes to the next player
    // /// If there is only one player left after the hand is played,
    // /// the last player is the round loser and the round should end
    // /// </summary>
    // private void OnHandPlayed(Node<PlayerHandComponent> player, ref HandPlayedSignal args)
    // {
    //     NextHandPlayerTurn();
    //     
    //     if (player.Comp.Cards.Count == 0)
    //     {
    //         // Round winner is player if no current round winner
    //         _roundWinner ??= player;
    //         
    //         // Player is removed from the round since they have no cards left to play
    //         _roundPlayers.Remove(player);
    //
    //         // If the amount of players left is one or less, the round ends
    //         if (_roundPlayers.Count <= 1)
    //         {
    //             EndRound();
    //             return;
    //         }
    //         
    //         // If there are still more players left, the next player in line is in control
    //         _inControlOfHand = _currentPlayerTurn;
    //         return;
    //     }
    //
    //     // The player who just played the hand still has more cards, so they're in control of the hand
    //     _inControlOfHand = player;
    // }
    //
    // /// <summary>
    // /// Deal cards, determine player order, and set current player's turn
    // /// </summary>
    // public void StartRound()
    // {
    //     // Takes two to tango
    //     // Threesome to be somethin...
    //     // But a fourway to bust your doorway
    //     if (_roomPlayers.Count < 2)
    //         return;
    //
    //     var dealerIndex = _roundLoser != null ? _roomPlayers.IndexOf(_roundLoser.Value) : 0;
    //     _deckSystem.DealCards(_roomPlayers, dealerIndex);
    //     
    //     // _roundPlayers = _lobbyPlayers;
    //     DeterminePlayerOrder();
    //     var signal = new RoundStartSignal();
    //     _signalBus.EmitRoundStartSignal(ref signal);
    //     
    //     _roundWinner = null;
    //     _roundLoser = null;
    // }
    //
    // private void EndRound()
    // {
    //     _roundLoser = _currentPlayerTurn;
    //     
    //     var signal = new RoundEndSignal();
    //     _signalBus.EmitRoundEndSignal(ref signal);
    // }
    //
    //
    //
    // /// <summary>
    // /// Goes to the next player's turn who's still participating in the hand
    // /// </summary>
    // private void NextHandPlayerTurn()
    // {
    //     if (_currentPlayerTurn == null)
    //         return;
    //     
    //     var currentPlayerIndex = _handPlayers.IndexOf(_currentPlayerTurn.Value);
    //     _currentPlayerTurn = _handPlayers[(currentPlayerIndex + 1) % _handPlayers.Count];
    // }
}

public class TurnPassedSignal : UserSignalArgs
{
    public Node<PlayerHandComponent> Node;
    
    public TurnPassedSignal(Node<PlayerHandComponent> node)
    {
        Node = node;
    }
}

public class HandPlayedSignal : UserSignalArgs
{
    public Node<PlayerHandComponent> Node;

    public HandPlayedSignal(Node<PlayerHandComponent> node)
    {
        Node = node;
    }
}