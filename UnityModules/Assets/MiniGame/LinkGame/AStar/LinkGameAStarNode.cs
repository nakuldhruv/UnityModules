using System;
using UnityEngine;

namespace Nakul.LinkGame
{
    public class LinkGameAStarNode : IComparable<LinkGameAStarNode>
    {
        public Vector2Int Position { get; private set; }
        public int FScore { get; private set; }

        public LinkGameAStarNode(Vector2Int position, int fScore)
        {
            this.Position = position;
            this.FScore = fScore;
        }

        public int CompareTo(LinkGameAStarNode other) => FScore.CompareTo(other.FScore);
    }
}