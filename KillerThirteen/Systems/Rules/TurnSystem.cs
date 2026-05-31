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
    
    private List<Node<HandComponent>> _players = [];
    private List<Node<HandComponent>> _orderedPlayersInRound = new();
    private Node<HandComponent>? _roundWinner;
    private Node<HandComponent>? _roundLoser;
    
    public void JoinGame(Node<HandComponent> player)
    {
        _players.Add(player);
    }

    public void LeaveGame(Node<HandComponent> player)
    {
        _players.Remove(player);
    }
    
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
    }
    
    // <summary>
    // Starter player in the order is determined by
    // 1. If there was a round winner, they start first and have freedom to play whatever
    // 2. Whoever has the three of spades gets to start
    // Player order goes clock-wise beginning with the starting player
    // </summary>
    private void DeterminePlayerOrder()
    {
        _orderedPlayersInRound.Clear();
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
            _orderedPlayersInRound.Add(_players[(i + startingPlayerIndex) % _players.Count]);
    }
}

public class RoundStartSignal : UserSignalArgs;