using System;
using UnityEngine;

namespace Nakul.LinkGame
{
    public enum LinkGameMapNodeType
    {
        Green,
        Red,
        Blue,
        Yellow,
        Magenta,
    }
    
    public static class LinkGameMapNodeTypeExtension
    {
        public static Color GetColor(this LinkGameMapNodeType type)
        {
            return type switch
            {
                LinkGameMapNodeType.Green => Color.green,
                LinkGameMapNodeType.Red => Color.red,
                LinkGameMapNodeType.Blue => Color.blue,
                LinkGameMapNodeType.Yellow => Color.yellow,
                LinkGameMapNodeType.Magenta => Color.magenta,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}