using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Common
{
    public static class Layers
    {
        public enum Layer
        {
            Player0,
            Player1,
            Player2,
            Player3,
            Player4,
            Player5,
            Player6,
            Player7,
            Player8
        }

        public static readonly int Default = LayerMask.NameToLayer(nameof(Default));
        public static readonly int Environment = LayerMask.NameToLayer(nameof(Environment));
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int Player0 = LayerMask.NameToLayer("Player 0");
        public static readonly int Player1 = LayerMask.NameToLayer("Player 1");
        public static readonly int Player2 = LayerMask.NameToLayer("Player 2");
        public static readonly int Player3 = LayerMask.NameToLayer("Player 3");
        public static readonly int Player4 = LayerMask.NameToLayer("Player 4");
        public static readonly int Player5 = LayerMask.NameToLayer("Player 5");
        public static readonly int Player6 = LayerMask.NameToLayer("Player 6");
        public static readonly int Player7 = LayerMask.NameToLayer("Player 7");
        public static readonly int Player8 = LayerMask.NameToLayer("Player 8");
        public static readonly int TransparentFx = LayerMask.NameToLayer("TransparentFX");
        public static readonly int UI = LayerMask.NameToLayer(nameof(UI));
        public static readonly int Water = LayerMask.NameToLayer(nameof(Water));

        public static LayerMask FilterNonPlayer(LayerMask layerMask)
        {
            return layerMask & ~(1 << Default) & ~(1 << Environment) & ~(1 << IgnoreRaycast) & ~(1 << TransparentFx) &
                   ~(1 << UI) & ~(1 << Water);
        }
        public static bool OwnedByPlayer(Transform transform)
        {
            var players = new List<int>
            {
                Player0,
                Player1,
                Player2,
                Player3,
                Player4,
                Player5,
                Player6,
                Player7,
                Player8
            };
            return players.Contains(transform.gameObject.layer);
        }
        public static int ToInt(Layer layer) { return LayerMask.NameToLayer(layer.ToString().Insert(6, " ")); }
        public static string ToString(Layer layer) { return layer.ToString().Insert(6, " "); }
    }
}