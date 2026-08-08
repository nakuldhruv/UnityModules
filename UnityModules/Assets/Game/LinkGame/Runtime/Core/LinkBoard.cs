using System.Collections.Generic;
using UnityEngine;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 连连看棋盘的纯数据模型（不依赖任何 MonoBehaviour / 视图对象）。
    /// 维护节点类型的二维网格，并提供清空、统计等操作。
    /// </summary>
    public sealed class LinkBoard : ILinkBoard
    {
        private readonly NodeType[,] _grid;

        public int Width { get; }
        public int Height { get; }
        public int RemainingCount { get; private set; }

        public LinkBoard(int width, int height)
        {
            Width = width;
            Height = height;
            _grid = new NodeType[width, height];
        }

        /// <summary>批量填充类型数组，并重建存活计数。</summary>
        public void LoadTypes(IReadOnlyList<NodeType> types)
        {
            if (types == null || types.Count != Width * Height)
            {
                Debug.LogError($"[LinkBoard] 类型数量 {types?.Count} 与棋盘容量 {Width * Height} 不匹配。");
                return;
            }

            int index = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _grid[x, y] = types[index++];
                }
            }

            RemainingCount = CountAlive();
        }

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
        }

        public bool IsEmptyAt(Vector2Int pos)
        {
            return !IsInside(pos) || GetTypeAt(pos) == NodeType.None;
        }

        public NodeType GetTypeAt(Vector2Int pos)
        {
            if (!IsInside(pos))
            {
                return NodeType.None;
            }

            return _grid[pos.x, pos.y];
        }

        public void SetType(Vector2Int pos, NodeType type)
        {
            if (!IsInside(pos))
            {
                return;
            }

            NodeType old = _grid[pos.x, pos.y];
            _grid[pos.x, pos.y] = type;

            if (old == NodeType.None && type != NodeType.None)
            {
                RemainingCount++;
            }
            else if (old != NodeType.None && type == NodeType.None)
            {
                RemainingCount = Mathf.Max(0, RemainingCount - 1);
            }
        }

        public void ClearAt(Vector2Int pos)
        {
            SetType(pos, NodeType.None);
        }

        /// <summary>将棋盘重置为空棋盘。</summary>
        public void ClearAll()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _grid[x, y] = NodeType.None;
                }
            }

            RemainingCount = 0;
        }

        /// <summary>收集所有存活节点的坐标。</summary>
        public List<Vector2Int> CollectAlivePositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>(RemainingCount);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] != NodeType.None)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }

            return positions;
        }

        private int CountAlive()
        {
            int count = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y] != NodeType.None)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}