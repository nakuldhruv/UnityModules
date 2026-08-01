using System.Collections.Generic;
using UnityEngine;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 连连看寻路：支持 0 / 1 / 2 拐角，路径可延伸到棋盘外一圈虚拟空位。
    /// </summary>
    public class LinkPathfinding
    {
        private int _mapWidth;
        private int _mapHeight;
        private Node[,] _nodeMap;

        public void Initialize(int mapWidth, int mapHeight, Node[,] nodeMap)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _nodeMap = nodeMap;
        }

        /// <summary>
        /// 查找连接路径，返回路径点列表（含起点、拐点、终点）。无法连接时返回 null。
        /// </summary>
        public List<Vector2Int> FindPath(Node startNode, Node targetNode)
        {
            if (startNode == null || targetNode == null)
            {
                return null;
            }

            if (startNode == targetNode)
            {
                return null;
            }

            if (startNode.Type == NodeType.None || targetNode.Type == NodeType.None)
            {
                return null;
            }

            if (startNode.Type != targetNode.Type)
            {
                return null;
            }

            Vector2Int start = startNode.Position;
            Vector2Int end = targetNode.Position;

            if (TryStraight(start, end, out List<Vector2Int> path))
            {
                return path;
            }

            if (TryOneCorner(start, end, out path))
            {
                return path;
            }

            if (TryTwoCorners(start, end, out path))
            {
                return path;
            }

            return null;
        }

        /// <summary>0 拐角：同行或同列直线可达。</summary>
        private bool TryStraight(Vector2Int start, Vector2Int end, out List<Vector2Int> path)
        {
            path = null;

            if (start.x != end.x && start.y != end.y)
            {
                return false;
            }

            if (!IsLineClear(start, end, start, end))
            {
                return false;
            }

            path = new List<Vector2Int> { start, end };
            return true;
        }

        /// <summary>1 拐角：L 形路径，拐点为 (start.x, end.y) 或 (end.x, start.y)。</summary>
        private bool TryOneCorner(Vector2Int start, Vector2Int end, out List<Vector2Int> path)
        {
            path = null;

            Vector2Int[] corners =
            {
                new Vector2Int(start.x, end.y),
                new Vector2Int(end.x, start.y),
            };

            foreach (Vector2Int corner in corners)
            {
                if (corner == start || corner == end)
                {
                    continue;
                }

                if (!IsPassable(corner, start, end))
                {
                    continue;
                }

                if (IsLineClear(start, corner, start, end) && IsLineClear(corner, end, start, end))
                {
                    path = new List<Vector2Int> { start, corner, end };
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 2 拐角：从起点沿行或列延伸到 pivot，再从 pivot 以 0 或 1 拐角连到终点。
        /// </summary>
        private bool TryTwoCorners(Vector2Int start, Vector2Int end, out List<Vector2Int> path)
        {
            path = null;

            for (int x = -1; x <= _mapWidth; x++)
            {
                Vector2Int pivot = new Vector2Int(x, start.y);
                if (TryPivotPath(start, end, pivot, out path))
                {
                    return true;
                }
            }

            for (int y = -1; y <= _mapHeight; y++)
            {
                Vector2Int pivot = new Vector2Int(start.x, y);
                if (TryPivotPath(start, end, pivot, out path))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryPivotPath(Vector2Int start, Vector2Int end, Vector2Int pivot, out List<Vector2Int> path)
        {
            path = null;

            if (pivot == start)
            {
                return false;
            }

            if (!IsPassable(pivot, start, end) || !IsLineClear(start, pivot, start, end))
            {
                return false;
            }

            if (TryStraight(pivot, end, out List<Vector2Int> tail))
            {
                path = new List<Vector2Int> { start, pivot };
                path.AddRange(tail.GetRange(1, tail.Count - 1));
                return true;
            }

            if (TryOneCorner(pivot, end, out tail))
            {
                path = new List<Vector2Int> { start, pivot };
                path.AddRange(tail.GetRange(1, tail.Count - 1));
                return true;
            }

            return false;
        }

        /// <summary>检查 a → b 之间的所有中间格是否可通过（不含 a、b 本身）。</summary>
        private bool IsLineClear(Vector2Int a, Vector2Int b, Vector2Int start, Vector2Int end)
        {
            if (a.x == b.x)
            {
                int minY = Mathf.Min(a.y, b.y);
                int maxY = Mathf.Max(a.y, b.y);
                for (int y = minY + 1; y < maxY; y++)
                {
                    if (!IsPassable(new Vector2Int(a.x, y), start, end))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (a.y == b.y)
            {
                int minX = Mathf.Min(a.x, b.x);
                int maxX = Mathf.Max(a.x, b.x);
                for (int x = minX + 1; x < maxX; x++)
                {
                    if (!IsPassable(new Vector2Int(x, a.y), start, end))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 格子是否可通过。棋盘外一圈（-1 / width / height）视为空位；
        /// 棋盘内已消除（None 或已销毁）的格子视为空位。
        /// </summary>
        private bool IsPassable(Vector2Int pos, Vector2Int start, Vector2Int end)
        {
            if (pos == start || pos == end)
            {
                return true;
            }

            if (IsOutsideBorder(pos))
            {
                return false;
            }

            if (IsVirtualBorder(pos))
            {
                return true;
            }

            Node node = _nodeMap[pos.x, pos.y];
            return node == null || node.Type == NodeType.None;
        }

        /// <summary>棋盘外一圈虚拟空位：x ∈ [-1, width]，y ∈ [-1, height] 的边界。</summary>
        private bool IsVirtualBorder(Vector2Int pos)
        {
            return pos.x == -1 || pos.x == _mapWidth
                || pos.y == -1 || pos.y == _mapHeight;
        }

        private bool IsOutsideBorder(Vector2Int pos)
        {
            return pos.x < -1 || pos.x > _mapWidth
                || pos.y < -1 || pos.y > _mapHeight;
        }
    }
}
