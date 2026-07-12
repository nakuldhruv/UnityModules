using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityModules
{
    /// <summary>
    /// 为 LinkGame（连连看）设计的 A* 寻路器。
    /// 支持拐角数限制，并在搜索时对拐角施加惩罚，对直线给予奖励，以倾向选择更直的路径。
    /// </summary>
    public class LinkGameAStarPathfinder
    {
        // -------------------- 配置参数 --------------------
        private static int _maxCornerCount;
        private static int _cornerPunishment;
        private static int _straightReward;

        // -------------------- 地图数据 --------------------
        private int _mapWidth;
        private int _mapHeight;
        private bool[,] _obstacleMap;   // true = 障碍物，不可通行

        // 四个基本移动方向（上、下、左、右）
        private readonly List<Vector2Int> _directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        /// <summary>
        /// 初始化寻路器，设置参数并创建地图数据（默认全部可通行）。
        /// </summary>
        public void Initialize(int maxCornerCount, int cornerPunishment, int straightReward, int xSize, int ySize)
        {
            _maxCornerCount = maxCornerCount;
            _cornerPunishment = cornerPunishment;
            _straightReward = straightReward;

            _mapWidth = xSize;
            _mapHeight = ySize;
            _obstacleMap = new bool[xSize, ySize]; // 默认 false（全部可通行）
        }

        /// <summary>
        /// 设置某个坐标是否为障碍物。
        /// </summary>
        public void SetObstacle(int x, int y, bool isObstacle)
        {
            if (IsInsideMapBounds(new Vector2Int(x, y)))
                _obstacleMap[x, y] = isObstacle;
        }

        /// <summary>
        /// 批量设置障碍物地图（外部直接赋值，便于从外部数据加载）。
        /// </summary>
        public void SetObstacleMap(bool[,] obstacleMap)
        {
            if (obstacleMap == null)
                throw new ArgumentNullException(nameof(obstacleMap));
            if (obstacleMap.GetLength(0) != _mapWidth || obstacleMap.GetLength(1) != _mapHeight)
                throw new ArgumentException("障碍物地图尺寸与初始化尺寸不匹配");
            _obstacleMap = obstacleMap;
        }

        /// <summary>
        /// 尝试寻找从起点到终点的路径。
        /// </summary>
        /// <param name="startNode">起点地图节点</param>
        /// <param name="targetNode">终点地图节点</param>
        /// <param name="resultPath">找到的路径（若成功），否则为 null</param>
        /// <returns>是否找到满足拐角限制的路径</returns>
        public bool TryFindPath(LinkGameMapNode startNode, LinkGameMapNode targetNode, out List<Vector2Int> resultPath)
        {
            resultPath = null;

            Vector2Int startPos = new Vector2Int(startNode.X, startNode.Y);
            Vector2Int targetPos = new Vector2Int(targetNode.X, targetNode.Y);

            // 1. 获取起点周围所有可行的邻居（第一步可走的位置）
            List<Vector2Int> startNeighbors = GetWalkableNeighbors(startPos, startNode, targetNode);
            if (startNeighbors.Count == 0)
                return false;

            // 2. 对每个可行的第一步方向，分别执行 A* 搜索
            List<List<Vector2Int>> allPaths = new List<List<Vector2Int>>();
            foreach (Vector2Int firstStep in startNeighbors)
            {
                List<Vector2Int> path = FindPathWithFirstStep(startPos, firstStep, targetPos, startNode, targetNode);
                if (path != null)
                    allPaths.Add(path);
            }

            if (allPaths.Count == 0)
                return false;

            // 3. 选择拐角数最少的路径
            List<Vector2Int> bestPath = null;
            int minCorners = int.MaxValue;
            foreach (List<Vector2Int> path in allPaths)
            {
                int corners = CalculatePathCorners(path);
                if (corners < minCorners)
                {
                    minCorners = corners;
                    bestPath = path;
                }
            }

            if (bestPath == null || minCorners > _maxCornerCount)
                return false;

            resultPath = bestPath;
            return true;
        }

        /// <summary>
        /// 强制从起点出发的第一步为指定邻居，执行 A* 搜索，返回完整路径。
        /// </summary>
        private List<Vector2Int> FindPathWithFirstStep(
            Vector2Int start,
            Vector2Int firstStep,
            Vector2Int target,
            LinkGameMapNode startNode,
            LinkGameMapNode targetNode)
        {
            PriorityQueue<LinkGameAStarNode> openQueue = new PriorityQueue<LinkGameAStarNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, int> gScores = new Dictionary<Vector2Int, int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            closedSet.Add(start);
            gScores[firstStep] = 1;
            cameFrom[firstStep] = start;
            int heuristic = CalculateHeuristic(firstStep, target);
            openQueue.Enqueue(new LinkGameAStarNode(firstStep, gScores[firstStep] + heuristic));

            while (openQueue.Count > 0)
            {
                LinkGameAStarNode current = openQueue.Dequeue();
                Vector2Int currentPos = current.Position;

                if (currentPos == target)
                    return ReconstructPath(cameFrom, currentPos);

                closedSet.Add(currentPos);

                foreach (Vector2Int dir in _directions)
                {
                    Vector2Int neighbor = currentPos + dir;

                    if (!IsInsideMapBounds(neighbor))
                        continue;
                    if (!IsWalkable(neighbor, startNode, targetNode))
                        continue;
                    if (closedSet.Contains(neighbor))
                        continue;

                    int tentativeG = gScores[currentPos] + 1;

                    if (cameFrom.TryGetValue(currentPos, out Vector2Int parent))
                    {
                        Vector2Int dirFromParent = currentPos - parent;
                        if (dir != dirFromParent)
                            tentativeG += _cornerPunishment;
                        else
                            tentativeG -= _straightReward;
                    }

                    if (!gScores.ContainsKey(neighbor) || tentativeG < gScores[neighbor])
                    {
                        cameFrom[neighbor] = currentPos;
                        gScores[neighbor] = tentativeG;
                        int fScore = tentativeG + CalculateHeuristic(neighbor, target);
                        openQueue.Enqueue(new LinkGameAStarNode(neighbor, fScore));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 计算一条路径中的拐角次数。
        /// </summary>
        private int CalculatePathCorners(List<Vector2Int> path)
        {
            if (path.Count < 3)
                return 0;

            int cornerCount = 0;
            Vector2Int previousDir = Vector2Int.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int currentDir = path[i] - path[i - 1];
                if (i == 1)
                {
                    previousDir = currentDir;
                    continue;
                }

                if (currentDir != previousDir)
                {
                    cornerCount++;
                    previousDir = currentDir;
                }
            }

            return cornerCount;
        }

        /// <summary>
        /// 从 cameFrom 字典重建路径（从起点到终点）。
        /// </summary>
        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> path = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// 曼哈顿距离启发式函数。
        /// </summary>
        private int CalculateHeuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 获取某个位置周围所有可通行的邻居（不包括自身）。
        /// </summary>
        private List<Vector2Int> GetWalkableNeighbors(Vector2Int pos, LinkGameMapNode startNode, LinkGameMapNode targetNode)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            foreach (Vector2Int dir in _directions)
            {
                Vector2Int neighbor = pos + dir;
                if (IsInsideMapBounds(neighbor) && IsWalkable(neighbor, startNode, targetNode))
                    neighbors.Add(neighbor);
            }
            return neighbors;
        }

        // -------------------- 地图通行性判断（具体实现） --------------------

        /// <summary>
        /// 检查坐标是否在地图范围内。
        /// </summary>
        public bool IsInsideMapBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _mapWidth &&
                   pos.y >= 0 && pos.y < _mapHeight;
        }

        /// <summary>
        /// 检查某个位置是否可通行（非障碍物）。
        /// 起点和终点虽然传入列表，但此处不做特殊排除，因为起点在搜索中被封闭，终点允许通过。
        /// </summary>
        public bool IsWalkable(Vector2Int pos, LinkGameMapNode startNode, LinkGameMapNode targetNode)
        {
            // 首先检查是否在边界内
            if (!IsInsideMapBounds(pos))
                return false;

            // 检查是否为障碍物（false = 可通行）
            return !_obstacleMap[pos.x, pos.y];
        }
    }
}