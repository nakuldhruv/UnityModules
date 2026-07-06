using System;
using UnityEngine;

namespace UnityModules
{
    public class AStarNode : IComparable<AStarNode>
    {
        public Vector2Int Position { get; private set; }
        public int FScore { get; private set; }

        public AStarNode(Vector2Int position, int fScore)
        {
            this.Position = position;
            this.FScore = fScore;
        }

        public int CompareTo(AStarNode other) => FScore.CompareTo(other.FScore);
    }
}