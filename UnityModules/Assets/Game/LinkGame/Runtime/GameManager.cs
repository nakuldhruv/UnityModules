using System.Collections;
using System.Collections.Generic;
using Nakul.Core;
using UnityEngine;

namespace Nakul.LinkGame
{
    public class GameManager : MonoBehaviour
    {
        private const float NodeSize = 100f;

        [SerializeField] private int _mapWidth = 6;
        [SerializeField] private int _mapHeight = 8;
        [SerializeField] private Node _nodePrefab;
        [SerializeField] private Transform _nodeParent;
        [SerializeField] private int _spacing = 10;
        [SerializeField] private int _xPadding;
        [SerializeField] private int _yPadding;
        [SerializeField] private NodeConfigSO _nodeConfigSo;
        [SerializeField] private float _lineThickness = 10f;
        [SerializeField] private float _lineDuration = 0.22f;
        [SerializeField] private Color _lineColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private float _spawnDuration = 0.3f;
        [SerializeField] private float _clearDuration = 0.15f;





        private LinkPathfinding _pathfinding;
        private LinkLineGraphic _lineGraphic;
        private Node[,] _nodeMap;
        private Node _selectedNode;
        private int _remainingCount;
        private bool _isBusy;
        private Coroutine _clearRoutine;
        private Coroutine _spawnRoutine;


        private void Awake()
        {
            StartGame();
        }

        public void StartGame()
        {
            if (_clearRoutine != null)
            {
                StopCoroutine(_clearRoutine);
                _clearRoutine = null;
            }

            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            ClearBoard();

            int total = _mapWidth * _mapHeight;
            if (total % 2 != 0)
            {
                this.Error($"棋盘格子数 {total} 必须为偶数。");
                return;
            }

            var types = NodeTypeHelper.GeneratePairedTypes(total);
            _pathfinding = new LinkPathfinding();
            _nodeMap = new Node[_mapWidth, _mapHeight];
            _remainingCount = total;
            _selectedNode = null;
            _isBusy = false;

            EnsureLineGraphic();

            _spawnRoutine = StartCoroutine(SpawnBoardRoutine(types));
        }

        /// <summary>
        /// 边生成边播放：逐个创建节点，每创建一个节点暂停一帧，
        /// 节点创建后立即播放生成动画，形成渐进生成的波浪效果。
        /// </summary>
        private IEnumerator SpawnBoardRoutine(List<NodeType> types)
        {
            _isBusy = true;

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    Node node = Instantiate(_nodePrefab, _nodeParent);
                    node.Position = new Vector2Int(x, y);
                    _nodeMap[x, y] = node;

                    node.GetComponent<RectTransform>().anchoredPosition = GetCellBottomLeft(x, y);

                    int index = x * _mapHeight + y;
                    NodeType type = types[index];
                    Sprite sprite = _nodeConfigSo.GetSprite(type);
                    node.Initialize(OnClickNode, type, sprite);
                    node.gameObject.name = $"Node_{x}_{y}_{type}";

                    // 创建后立即播放生成动画
                    node.PlaySpawnAnimation(_spawnDuration, 0f);

                    // 每创建一个节点暂停一帧，实现边生成边播放
                    yield return null;
                }
            }

            _pathfinding.Initialize(_mapWidth, _mapHeight, _nodeMap);

            _isBusy = false;
            _spawnRoutine = null;

            EnsurePlayable();
        }




        private void OnClickNode(Node clickNode)
        {
            if (_isBusy || clickNode == null || clickNode.Type == NodeType.None)
            {
                return;
            }

            if (_selectedNode == null)
            {
                SelectNode(clickNode);
                return;
            }

            if (clickNode == _selectedNode)
            {
                ClearSelection();
                return;
            }

            if (_selectedNode.Type != clickNode.Type)
            {
                SelectNode(clickNode);
                return;
            }

            List<Vector2Int> path = _pathfinding.FindPath(_selectedNode, clickNode);
            if (path == null)
            {
                this.Log("无法连接，切换选中。");
                SelectNode(clickNode);
                return;
            }

            this.Log($"消除成功，拐点数: {Mathf.Max(0, path.Count - 2)}");
            Node first = _selectedNode;
            Node second = clickNode;
            ClearSelection();
            _clearRoutine = StartCoroutine(ClearLinkedPair(first, second, path));
        }

        private IEnumerator ClearLinkedPair(Node first, Node second, List<Vector2Int> path)
        {
            _isBusy = true;

            yield return StartCoroutine(PlayLinkLineAnimation(path));
            yield return StartCoroutine(PlayClearAnimation(first, second));

            _isBusy = false;
            _clearRoutine = null;

            if (_remainingCount <= 0)
            {
                this.Log("全部消除，重新开局。");
                yield return new WaitForSeconds(0.35f);
                StartGame();
                yield break;
            }

            EnsurePlayable();
        }

        /// <summary>连线动画：连线沿路径渐进绘制，短暂停留后隐藏。</summary>
        private IEnumerator PlayLinkLineAnimation(List<Vector2Int> path)
        {
            ShowLinkLine(path);
            _lineGraphic.SetProgress(0f);

            float t = 0f;
            while (t < _lineDuration)
            {
                t += Time.deltaTime;
                _lineGraphic.SetProgress(t / _lineDuration);
                yield return null;
            }

            _lineGraphic.SetProgress(1f);
            yield return new WaitForSeconds(0.08f);
            HideLinkLine();
        }

        /// <summary>消除动画：两个节点同时缩小消失后销毁。</summary>
        private IEnumerator PlayClearAnimation(Node first, Node second)
        {
            first.PlayClearAnimation(_clearDuration, null);
            second.PlayClearAnimation(_clearDuration, null);

            yield return new WaitForSeconds(_clearDuration);

            ClearNode(first);
            ClearNode(second);
        }


        private void ShowLinkLine(List<Vector2Int> path)
        {
            EnsureLineGraphic();

            List<Vector2> points = new List<Vector2>(path.Count);
            for (int i = 0; i < path.Count; i++)
            {
                points.Add(GetCellCenter(path[i].x, path[i].y));
            }

            _lineGraphic.color = _lineColor;
            _lineGraphic.Thickness = _lineThickness;
            _lineGraphic.Show(points);
            _lineGraphic.transform.SetAsLastSibling();
        }

        private void HideLinkLine()
        {
            if (_lineGraphic != null)
            {
                _lineGraphic.Hide();
            }
        }

        private void EnsureLineGraphic()
        {
            if (_lineGraphic != null)
            {
                return;
            }

            GameObject go = new GameObject("LinkLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(LinkLineGraphic));
            go.layer = _nodeParent.gameObject.layer;
            go.transform.SetParent(_nodeParent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(4096f, 4096f);

            _lineGraphic = go.GetComponent<LinkLineGraphic>();
            _lineGraphic.color = _lineColor;
            _lineGraphic.Thickness = _lineThickness;
            _lineGraphic.raycastTarget = false;
            _lineGraphic.Hide();

        }

        private Vector2 GetCellBottomLeft(int x, int y)
        {
            float step = NodeSize + _spacing;
            return new Vector2(_xPadding + step * x, _yPadding + step * y);
        }

        private Vector2 GetCellCenter(int x, int y)
        {
            return GetCellBottomLeft(x, y) + new Vector2(NodeSize * 0.5f, NodeSize * 0.5f);
        }

        private void SelectNode(Node node)
        {
            if (_selectedNode != null)
            {
                _selectedNode.SetSelected(false);
            }

            _selectedNode = node;
            _selectedNode.SetSelected(true);
        }

        private void ClearSelection()
        {
            if (_selectedNode != null)
            {
                _selectedNode.SetSelected(false);
                _selectedNode = null;
            }
        }

        private void ClearNode(Node node)
        {
            if (node == null)
            {
                return;
            }

            Vector2Int pos = node.Position;
            if (_nodeMap != null
                && pos.x >= 0 && pos.x < _mapWidth
                && pos.y >= 0 && pos.y < _mapHeight
                && _nodeMap[pos.x, pos.y] == node)
            {
                _nodeMap[pos.x, pos.y] = null;
            }

            _remainingCount = Mathf.Max(0, _remainingCount - 1);

            // 先彻底隐藏再销毁，避免微信小游戏上 Destroy 延迟导致残影
            node.gameObject.SetActive(false);
            Destroy(node.gameObject);
        }


        private void EnsurePlayable()
        {
            if (_remainingCount <= 0)
            {
                return;
            }

            if (HasAnyLinkablePair())
            {
                return;
            }

            this.Log("无解，自动重排。");
            ShuffleRemaining();

            int guard = 0;
            while (!HasAnyLinkablePair() && guard < 20)
            {
                ShuffleRemaining();
                guard++;
            }

            if (!HasAnyLinkablePair())
            {
                this.Warning("重排后仍无解，请重新开始。");
            }
        }

        private bool HasAnyLinkablePair()
        {
            List<Node> nodes = CollectAliveNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    if (nodes[i].Type != nodes[j].Type)
                    {
                        continue;
                    }

                    if (_pathfinding.FindPath(nodes[i], nodes[j]) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ShuffleRemaining()
        {
            ClearSelection();

            List<Node> nodes = CollectAliveNodes();
            List<NodeType> types = new List<NodeType>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                types.Add(nodes[i].Type);
            }

            NodeTypeHelper.Shuffle(types);

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeType type = types[i];
                nodes[i].ApplyType(type, _nodeConfigSo.GetSprite(type));
            }
        }

        private List<Node> CollectAliveNodes()
        {
            List<Node> nodes = new List<Node>(_remainingCount);
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    Node node = _nodeMap[x, y];
                    if (node != null && node.Type != NodeType.None)
                    {
                        nodes.Add(node);
                    }
                }
            }

            return nodes;
        }

        private void ClearBoard()
        {
            ClearSelection();
            HideLinkLine();

            if (_nodeMap == null)
            {
                return;
            }

            for (int x = 0; x < _nodeMap.GetLength(0); x++)
            {
                for (int y = 0; y < _nodeMap.GetLength(1); y++)
                {
                    Node node = _nodeMap[x, y];
                    if (node != null)
                    {
                        Destroy(node.gameObject);
                    }
                }
            }

            _nodeMap = null;
            _remainingCount = 0;
        }
    }
}
