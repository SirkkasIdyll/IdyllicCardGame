using System.Collections.Generic;
using Godot;
using Kolmetoista.Systems.Cards;
using Kolmetoista.Temperance.NCS;

namespace Kolmetoista.Systems.Player;

[GlobalClass]
public partial class PlayerHandComponent : Component
{
    public List<Node<CardComponent>> Cards = [];
}