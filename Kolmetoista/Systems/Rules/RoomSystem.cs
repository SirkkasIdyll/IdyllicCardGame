using System;
using System.Linq;
using Godot;
using Kolmetoista.Systems.Player;
using Kolmetoista.Temperance.NCS;
using Kolmetoista.Temperance.Signals;

namespace Kolmetoista.Systems.Rules;

/// <summary>
/// A room consists of the players available to play in a round
/// But does not necessarily mean they are in the round itself
/// </summary>
[GlobalClass]
public partial class RoomSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;

    public const int MaxPlayers = 4;

    /// <summary>
    /// List of players in the game, doesn't necessarily mean they're in the round
    /// All players are added to the round at the start of each round
    /// </summary>
    private readonly Node<PlayerHandComponent>?[] _playersInRoom = new Node<PlayerHandComponent>?[MaxPlayers];
    
    /// <summary>
    /// A player should be able to join a game at any time but not necessarily be in the hand
    /// They can be dealt in next round
    /// </summary>
    /// <param name="node"></param>
    public bool TryJoinRoom(Node<PlayerHandComponent> node)
    {
        for (var i = 0; i < MaxPlayers; i++)
        {
            if (!_playersInRoom[i].HasValue || _playersInRoom[i].Value != node)
                continue;
            
            var alreadyInRoomSignal = new FailedToJoinRoomSignal();
            _signalBus.EmitFailedToJoinRoomSignal(ref alreadyInRoomSignal);
            return false;
        }
        
        for (var i = 0; i < MaxPlayers; i++)
        {
            if (_playersInRoom[i] != null)
                continue;
            
            _playersInRoom[i] = node;
            var joinedSignal = new JoinedRoomSignal(node);
            _signalBus.EmitJoinedRoomSignal(node, ref joinedSignal);
            return true;
        }

        var failedSignal = new FailedToJoinRoomSignal();
        _signalBus.EmitFailedToJoinRoomSignal(ref failedSignal);
        return false;
    }

    /// <summary>
    /// A player should be able to leave a game at any time
    /// </summary>
    /// <param name="node"></param>
    public void LeaveRoom(Node<PlayerHandComponent> node)
    {
        foreach (var player in _playersInRoom)
        {
            if (player == null)
                continue;

            if (player.Value != node)
                continue;

            var index = _playersInRoom.IndexOf(node);
            _playersInRoom[index] = null;

            var signal = new LeaveRoomSignal(node);
            _signalBus.EmitLeaveRoomSignal(node, ref signal);
            return;
        }
    }

    public Node<PlayerHandComponent>?[] GetPlayersInRoom()
    {
        return _playersInRoom;
    }
}

public class JoinedRoomSignal : UserSignalArgs
{
    private Node<PlayerHandComponent> _player;

    public JoinedRoomSignal(Node<PlayerHandComponent> player)
    {
        _player = player;
    }
}

public class FailedToJoinRoomSignal : UserSignalArgs;

public class LeaveRoomSignal : UserSignalArgs
{
    private Node<PlayerHandComponent> _player;

    public LeaveRoomSignal(Node<PlayerHandComponent> player)
    {
        _player = player;
    }
}