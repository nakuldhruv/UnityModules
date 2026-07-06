using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityModules
{
    public class AStarPathFinder
    {
        private PriorityQueue<AStarNode> mOpenQueue = new PriorityQueue<AStarNode>();
        private HashSet<Vector2Int> mClosedSet = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, int> mGScores = new Dictionary<Vector2Int, int>();
        private Dictionary<Vector2Int, Vector2Int?> mPathOrigins = new Dictionary<Vector2Int, Vector2Int?>();
        private List<List<Vector2Int>> mPaths = new List<List<Vector2Int>>();
        private List<Vector2Int> mStartNeighborPos = new List<Vector2Int>();
        private List<Vector2Int> mTriedStartNeighborPos = new List<Vector2Int>();
        private List<Vector2Int> mPath = new List<Vector2Int>();
        private List<Vector2Int> mDirections = new List<Vector2Int>();
        
        private static int _maxCornerCount;
        private static int _cornerPunishment;
        private static int _straightReward;

        private bool TryFindPath(LinkGameNode startTile, LinkGameNode endTile)
        {
            this.mOpenQueue.Clear();
            this.mClosedSet.Clear();
            this.mGScores.Clear();
            this.mPathOrigins.Clear();
            this.mPaths.Clear();
            this.mStartNeighborPos.Clear();
            this.mStartNeighborPos.Clear();
            this.mPath = null;

            var startPos = new Vector2Int(startTile.X, startTile.Y);

            foreach (var direction in mDirections)
            {
                var neighborPos = startPos + direction;
                if (this.IsInsideMapBounds(neighborPos) && this.IsWalkable(neighborPos, new List<LinkGameNode>()
                    {
                        startTile,
                        endTile
                    }))
                    this.mStartNeighborPos.Add(neighborPos);
            }

            this.mGScores[startPos] = 0;
            this.mOpenQueue.Enqueue(new AStarNode(startPos, this.CalculateHeuristic(startPos, endTile)));

            int triedCount = 0;

            while (this.mOpenQueue.Count > 0)
            {
                var currentNode = mOpenQueue.Dequeue();
                var currentPos = currentNode.Position;

                if (currentPos.x == endTile.X && currentPos.y == endTile.Y)
                {
                    this.mPath = this.ReconstructPath(this.mPathOrigins, currentPos);
                    this.mPaths.Add(this.mPath);

                    if (++triedCount == this.mStartNeighborPos.Count) break;

                    this.mOpenQueue.Clear();
                    this.mClosedSet.Clear();
                    this.mGScores.Clear();
                    this.mPathOrigins.Clear();

                    this.mStartNeighborPos.Add(this.mPath[1]);
                    this.mGScores[startPos] = 0;
                    this.mOpenQueue.Enqueue(new AStarNode(startPos, this.CalculateHeuristic(startPos, endTile)));
                    continue;
                }

                this.mClosedSet.Add(currentPos);

                foreach (var direction in this.mDirections)
                {
                    var neighborPos = currentPos + direction;

                    if (!this.IsInsideMapBounds(neighborPos)) continue;
                    if (!this.IsWalkable(neighborPos, new List<LinkGameNode>() { startTile, endTile })) continue;
                    if (this.mClosedSet.Contains(neighborPos)) continue;
                    if (this.mStartNeighborPos.Contains(neighborPos)) continue;

                    int tentativeGScore = 0;

                    if (neighborPos != endTile.Position)
                    {
                        tentativeGScore = this.mGScores[currentPos] + 1;
                        bool hasParent = this.mPathOrigins.TryGetValue(currentPos, out
                            var parent);
                        if (hasParent && parent.HasValue)
                            if (direction != currentPos - parent.Value)
                                tentativeGScore += _cornerPunishment;
                            else
                                tentativeGScore -= _straightReward;
                    }

                    if (!mGScores.ContainsKey(neighborPos) || tentativeGScore < this.mGScores[neighborPos])
                    {
                        this.mPathOrigins[neighborPos] = currentPos;
                        this.mGScores[neighborPos] = tentativeGScore;
                        int fScore = tentativeGScore + this.CalculateHeuristic(neighborPos, endTile);
                        this.mOpenQueue.Enqueue(new AStarNode(neighborPos, fScore));
                    }
                }
            }

            if (this.mPath == null)
                return false;

            foreach (var path in this.mPaths)
                if (this.CalculatePathCorners(this.mPath) > this.CalculatePathCorners(path))
                    this.mPath = path;

            return this.CalculatePathCorners(this.mPath) <= _maxCornerCount;
        }

        private int CalculatePathCorners(List<Vector2Int> path)
        {
            int cornerCount = 0;
            Vector2 currentDir =
                default;

            for (int i = 0; i < path.Count; i++)
            {
                if (i + 1 >= path.Count) return cornerCount;

                if (currentDir == default)
                {
                    currentDir = path[i + 1] - path[i];
                    continue;
                }

                var newDir = path[i + 1] - path[i];
                if (newDir != currentDir)
                {
                    currentDir = newDir;
                    cornerCount++;
                }
            }

            return cornerCount;
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int?> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };

            while (cameFrom.ContainsKey(current) && cameFrom[current] != null)
            {
                current = cameFrom[current].Value;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private int CalculateHeuristic(Vector2Int a, LinkGameNode b) =>
            Mathf.Abs(a.x - b.X) + Mathf.Abs(a.y - b.Y);

        public bool IsInsideMapBounds(Vector2Int pos)
        {
            return true;
        }

        public bool IsWalkable(Vector2Int rpos, List<LinkGameNode> pos)
        {
            return true;
        }
    }
}