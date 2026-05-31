using System.Collections.Generic;
using Godot;
using KillerThirteen.Systems.Cards;
using KillerThirteen.Temperance.NCS;

namespace KillerThirteen.Systems.Rules;

[GlobalClass]
public partial class HandComponent : Component
{
    public List<Node<CardComponent>> Cards = [];
}