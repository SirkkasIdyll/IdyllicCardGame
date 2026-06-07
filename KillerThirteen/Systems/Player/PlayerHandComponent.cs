using System.Collections.Generic;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Player;

[GlobalClass]
public partial class PlayerHandComponent : Component
{
    public List<Node<CardComponent>> Cards = [];
}