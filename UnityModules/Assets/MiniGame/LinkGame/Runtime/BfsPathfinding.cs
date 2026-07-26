using System.Collections.Generic;
using UnityEngine;

namespace Nakul.LinkGame
{
    public class BfsPathfinding
    {
        private int _mapWidth;
        private int _mapHeight;
        private Node[,] _nodeMap;

        private readonly Queue<Node> _searchQueue = new Queue<Node>();
        private readonly HashSet<Node> _visited = new HashSet<Node>();
        private readonly List<Node> _neighborsBuffer = new List<Node>();

        private readonly List<Vector2Int> _directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        public void Initialize(int mapWidth, int mapHeight, Node[,] nodeMap)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _nodeMap = nodeMap;
        }

        public List<Node> FindPath(Node startNode, Node targetNode)
        {
            // !startNode.IsWalkable || !targetNode.IsWalkable ||  -> 判断Type
            if (!IsInsideMap(startNode.Position) || !IsInsideMap(targetNode.Position))
            {
                Debug.LogError("无效起点和终点。");
                return null;
            }

            _searchQueue.Enqueue(startNode);
            _visited.Add(startNode);

            while (_searchQueue.Count > 0)
            {
                Node node = _searchQueue.Dequeue();
                if (node.Position == targetNode.Position)
                {
                    return RetracePath(targetNode, startNode);
                }

                RefreshNeighbors(node);
                foreach (var neighbor in _neighborsBuffer)
                {
                    if (!_visited.Contains(neighbor))
                    {
                        _searchQueue.Enqueue(neighbor);
                        _visited.Add(neighbor);
                        neighbor.Parent = node;

                        if (neighbor.Position == targetNode.Position)
                        {
                            return RetracePath(targetNode, startNode);
                        }
                    }
                }
            }

            return null;
        }

        private List<Node> RetracePath(Node targetNode, Node startNode)
        {
            List<Node> path = new List<Node>();
            Node node = targetNode;
            path.Add(targetNode);
            while (node.Position != startNode.Position)
            {
                node = node.Parent;
                path.Add(node);
            }

            return path;
        }

        private void RefreshNeighbors(Node node)
        {
            _neighborsBuffer.Clear();
            foreach (var direction in _directions)
            {
                Vector2Int neighborPos = node.Position + direction;
                // _nodeMap[neighborPos.x, neighborPos.y].IsWalkable
                if (IsInsideMap(neighborPos) && !_visited.Contains(_nodeMap[neighborPos.x, neighborPos.y]))
                {
                    _neighborsBuffer.Add(_nodeMap[neighborPos.x, neighborPos.y]);
                }
            }
        }

        private bool IsInsideMap(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _mapWidth && pos.y >= 0 && pos.y < _mapHeight;
        }
    }
}