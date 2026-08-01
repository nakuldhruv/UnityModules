using System;
using System.Collections;
using System.Collections.Generic;
using Nakul.Core;
using UnityEngine;
using UnityEngine.UI;


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
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _totalScoreText;

        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioSource _audioSource;


        // 积分配置
        private const int BaseScore = 10;          // 每次成功连接的基础分
        private const int TurnBonusPerLevel = 5;   // 每少一个拐点额外加分
        private const int MaxTurnBonusLevel = 3;   // 拐点少于该值才可获得拐点奖励
        private const int ComboBonusPerLevel = 2;  // 每层连击额外加分

        // 关卡配置
        private const int MaxLevel = 999;          // 最大关卡数
        private const string LevelPrefsKey = "LinkGame_Level"; // 关卡存档键
        private const string TotalScorePrefsKey = "LinkGame_TotalScore"; // 总分存档键


        /// <summary>节点生成顺序模式。</summary>
        private enum SpawnPattern
        {
            RowByRow,      // 逐行：从左到右、从下到上
            ColumnByColumn,// 逐列：从下到上、从左到右
            OutsideIn,     // 从外到内：螺旋向中心
            InsideOut,     // 从内到外：螺旋向外
            Random,        // 随机顺序
        }




        /// <summary>当前总分。</summary>
        public int Score => _score;

        /// <summary>当前连击数（连续成功消除的次数）。</summary>
        public int Combo => _combo;

        /// <summary>积分变化事件，参数为当前总分。</summary>
        public event Action<int> OnScoreChanged;






        private LinkPathfinding _pathfinding;
        private LinkLineGraphic _lineGraphic;
        private Node[,] _nodeMap;
        private Node _selectedNode;
        private int _remainingCount;
        private int _score;
        private int _totalScore;
        private int _combo;
        private int _currentLevel;
        private bool _isBusy;

        private Coroutine _clearRoutine;
        private Coroutine _spawnRoutine;



        private void Awake()
        {
            LoadLevel();
            LoadTotalScore();
            StartGame();
        }


        private void Update()
        {
#if UNITY_EDITOR
            // 编辑器调试：按空格自动连接一对可消除的节点
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.Log("空格按下，触发自动连接。");
                AutoConnect();
            }
#endif
        }

        /// <summary>
        /// 自动寻找一对可连接的节点并消除，用于编辑器调试。
        /// </summary>
        private void AutoConnect()
        {
            if (_isBusy)
            {
                this.Log("自动连接被忽略：当前正在播放动画（_isBusy=true）。");
                return;
            }

            if (_nodeMap == null || _remainingCount <= 0)
            {
                this.Log("自动连接被忽略：棋盘为空或未初始化。");
                return;
            }


            List<Node> nodes = CollectAliveNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    if (nodes[i].Type != nodes[j].Type)
                    {
                        continue;
                    }

                    List<Vector2Int> path = _pathfinding.FindPath(nodes[i], nodes[j]);
                    if (path == null)
                    {
                        continue;
                    }

                    // 找到一对可连接的节点，模拟点击消除
                    this.Log($"自动连接: ({nodes[i].Position.x},{nodes[i].Position.y}) -> ({nodes[j].Position.x},{nodes[j].Position.y})");
                    int turns = Mathf.Max(0, path.Count - 2);
                    AddScore(CalculateScore(turns));
                    Node first = nodes[i];
                    Node second = nodes[j];
                    ClearSelection();
                    _clearRoutine = StartCoroutine(ClearLinkedPair(first, second, path));
                    return;
                }
            }

            this.Log("自动连接：当前无解，尝试重排。");
            EnsurePlayable();
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
            _score = 0;
            _combo = 0;
            UpdateScoreText();

            EnsureLineGraphic();


            _spawnRoutine = StartCoroutine(SpawnBoardRoutine(types));
        }

        /// <summary>
        /// 边生成边播放：每次进入随机选择一种生成顺序模式，
        /// 逐个创建节点，每创建一个节点暂停一帧，形成不同的渐进生成特效。
        /// </summary>
        private IEnumerator SpawnBoardRoutine(List<NodeType> types)
        {
            _isBusy = true;

            // 每次进入随机选择一种生成顺序模式
            SpawnPattern pattern = (SpawnPattern)UnityEngine.Random.Range(0, Enum.GetValues(typeof(SpawnPattern)).Length);
            List<Vector2Int> order = GenerateSpawnOrder(pattern);
            this.Log($"生成特效模式: {pattern}");

            for (int i = 0; i < order.Count; i++)
            {
                Vector2Int pos = order[i];
                int x = pos.x;
                int y = pos.y;

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

            _pathfinding.Initialize(_mapWidth, _mapHeight, _nodeMap);

            _isBusy = false;
            _spawnRoutine = null;

            EnsurePlayable();
        }

        /// <summary>
        /// 根据指定模式生成节点的生成顺序（坐标列表）。
        /// </summary>
        private List<Vector2Int> GenerateSpawnOrder(SpawnPattern pattern)
        {
            List<Vector2Int> order = new List<Vector2Int>(_mapWidth * _mapHeight);

            switch (pattern)
            {
                case SpawnPattern.RowByRow:
                    for (int x = 0; x < _mapWidth; x++)
                    {
                        for (int y = 0; y < _mapHeight; y++)
                        {
                            order.Add(new Vector2Int(x, y));
                        }
                    }
                    break;

                case SpawnPattern.ColumnByColumn:
                    for (int y = 0; y < _mapHeight; y++)
                    {
                        for (int x = 0; x < _mapWidth; x++)
                        {
                            order.Add(new Vector2Int(x, y));
                        }
                    }
                    break;

                case SpawnPattern.OutsideIn:
                    order = GenerateSpiralOrder(true);
                    break;

                case SpawnPattern.InsideOut:
                    order = GenerateSpiralOrder(false);
                    break;

                case SpawnPattern.Random:
                    for (int x = 0; x < _mapWidth; x++)
                    {
                        for (int y = 0; y < _mapHeight; y++)
                        {
                            order.Add(new Vector2Int(x, y));
                        }
                    }
                    NodeTypeHelper.Shuffle(order);
                    break;
            }

            return order;
        }

        /// <summary>
        /// 生成螺旋顺序。outsideIn 为 true 时从外圈向中心，false 时从中心向外圈。
        /// </summary>
        private List<Vector2Int> GenerateSpiralOrder(bool outsideIn)
        {
            List<Vector2Int> spiral = new List<Vector2Int>(_mapWidth * _mapHeight);

            int top = 0;
            int bottom = _mapHeight - 1;
            int left = 0;
            int right = _mapWidth - 1;

            while (top <= bottom && left <= right)
            {
                // 上边：从左到右
                for (int x = left; x <= right; x++)
                {
                    spiral.Add(new Vector2Int(x, top));
                }
                top++;

                // 右边：从上到下
                for (int y = top; y <= bottom; y++)
                {
                    spiral.Add(new Vector2Int(right, y));
                }
                right--;

                if (top <= bottom)
                {
                    // 下边：从右到左
                    for (int x = right; x >= left; x--)
                    {
                        spiral.Add(new Vector2Int(x, bottom));
                    }
                    bottom--;
                }

                if (left <= right)
                {
                    // 左边：从下到上
                    for (int y = bottom; y >= top; y--)
                    {
                        spiral.Add(new Vector2Int(left, y));
                    }
                    left++;
                }
            }

            if (outsideIn)
            {
                return spiral;
            }

            // 从内到外：反转螺旋顺序
            spiral.Reverse();
            return spiral;
        }





        private void OnClickNode(Node clickNode)
        {
            if (_isBusy || clickNode == null || clickNode.Type == NodeType.None)
            {
                return;
            }

            PlayClickSound();

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
                ResetCombo();
                SelectNode(clickNode);
                return;
            }

            int turns = Mathf.Max(0, path.Count - 2);
            this.Log($"消除成功，拐点数: {turns}");
            AddScore(CalculateScore(turns));
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
                this.Log("全部消除，进入下一关。");
                SettleLevelScore();
                AdvanceLevel();
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

        /// <summary>
        /// 计算一次成功连接获得的积分。
        /// 基础分 + 拐点奖励（拐点越少分越高）+ 连击奖励。
        /// </summary>
        private int CalculateScore(int turns)
        {
            int turnBonus = Mathf.Max(0, MaxTurnBonusLevel - turns) * TurnBonusPerLevel;
            int comboBonus = _combo * ComboBonusPerLevel;
            return BaseScore + turnBonus + comboBonus;
        }

        /// <summary>累加积分并刷新 UI、触发事件。</summary>
        private void AddScore(int points)
        {
            _score += points;
            _combo++;
            UpdateScoreText();
            OnScoreChanged?.Invoke(_score);
        }

        /// <summary>连接失败时重置连击。</summary>
        private void ResetCombo()
        {
            _combo = 0;
        }

        /// <summary>刷新积分文本显示。</summary>
        private void UpdateScoreText()
        {
            if (_scoreText != null)
            {
                _scoreText.text = _score.ToString();
            }
        }

        /// <summary>从 PlayerPrefs 读取当前关卡，默认第 1 关。</summary>
        private void LoadLevel()
        {
            _currentLevel = PlayerPrefs.GetInt(LevelPrefsKey, 1);
            _currentLevel = Mathf.Clamp(_currentLevel, 1, MaxLevel);
            UpdateLevelText();
        }

        /// <summary>进入下一关，超过最大关卡后回到第 1 关，并保存进度。</summary>
        private void AdvanceLevel()
        {
            _currentLevel++;
            if (_currentLevel > MaxLevel)
            {
                _currentLevel = 1;
            }

            PlayerPrefs.SetInt(LevelPrefsKey, _currentLevel);
            PlayerPrefs.Save();
            UpdateLevelText();
        }

        /// <summary>刷新关卡文本显示。</summary>
        private void UpdateLevelText()
        {
            if (_levelText != null)
            {
                _levelText.text = $"第 {_currentLevel} 关";
            }
        }

        /// <summary>从 PlayerPrefs 读取累计总分，默认 0。</summary>
        private void LoadTotalScore()
        {
            _totalScore = PlayerPrefs.GetInt(TotalScorePrefsKey, 0);
            UpdateTotalScoreText();
        }

        /// <summary>
        /// 关卡结束结算：将本关得分累加到总分并持久化存储。
        /// </summary>
        private void SettleLevelScore()
        {
            _totalScore += _score;
            PlayerPrefs.SetInt(TotalScorePrefsKey, _totalScore);
            PlayerPrefs.Save();
            UpdateTotalScoreText();
            this.Log($"关卡结算：本关得分 {_score}，累计总分 {_totalScore}");
        }

        /// <summary>刷新总分文本显示。</summary>
        private void UpdateTotalScoreText()
        {
            if (_totalScoreText != null)
            {
                _totalScoreText.text = _totalScore.ToString();
            }
        }


        /// <summary>播放点击音效。</summary>

        private void PlayClickSound()
        {
            if (_clickSound == null)
            {
                return;
            }

            EnsureAudioSource();
            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        /// <summary>确保存在可用的 AudioSource，未指定时自动挂载。</summary>
        private void EnsureAudioSource()
        {
            if (_audioSource != null)
            {
                return;
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
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
