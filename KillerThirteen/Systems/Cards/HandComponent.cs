using System.Collections.Generic;
using Godot;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Cards;

[GlobalClass]
public partial class HandComponent : Component
{
    public List<Node<CardComponent>> Cards = [];
}