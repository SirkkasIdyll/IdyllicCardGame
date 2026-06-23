using System.Collections.Generic;
using Godot;
using Kolmetoista.Systems.Player;
using Kolmetoista.Temperance.NCS;
using Kolmetoista.Temperance.Signals;

namespace Kolmetoista.Systems.Rules;

[GlobalClass]
public partial class RoomSystem : NodeSystem
{
    [InjectedDependency] private readonly SignalBus _signalBus = null!;

    /// <summary>
    /// List of players in the game, doesn't necessarily mean they're in the round
    /// All players are added to the round at the start of each round
    /// </summary>
    private readonly Node<PlayerHandComponent>?[] _playersInRoom = new Node<PlayerHandComponent>?[4];
    
    /// <summary>
    /// A player should be able to join a game at any time but not necessarily be in the hand
    /// They can be dealt in next round
    /// </summary>
    /// <param name="player"></param>
    public void JoinRoom(Node<PlayerHandComponent> player)
    {
        for (var i = 0; i < _playersInRoom.Length; i++)
        {
            if (_playersInRoom[i] != null)
                continue;
            
            _playersInRoom[i] = player;
            var joinedSignal = new JoinedRoomSignal(player);
            _signalBus.EmitJoinedRoomSignal(player, ref joinedSignal);
            return;
        }

        var failedSignal = new FailedToJoinRoomSignal();
        _signalBus.EmitFailedToJoinRoomSignal(ref failedSignal);
    }

    /// <summary>
    /// A player should be able to leave a game at any time
    /// </summary>
    /// <param name="player"></param>
    public void LeaveRoom(Node<PlayerHandComponent> player)
    {
        for (var i = 0; i < _playersInRoom.Length; i++)
        {
            if (_playersInRoom[i] == null)
                continue;
            
            if (!_playersInRoom[i].Value.Equals(player))
                continue;
            
            _playersInRoom[i] = null;
            var signal = new LeftRoomSignal(player);
            _signalBus.EmitLeftRoomSignal(player, ref signal);
            return;
        }
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

public class LeftRoomSignal : UserSignalArgs
{
    private Node<PlayerHandComponent> _player;

    public LeftRoomSignal(Node<PlayerHandComponent> player)
    {
        _player = player;
    }
}