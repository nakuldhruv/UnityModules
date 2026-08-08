using System;
using System.Collections;
using System.Collections.Generic;
using Nakul.Core;
using UnityEngine;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 游戏编排器：仅负责输入流转、胜负判定与各子系统之间的协调。
    /// 规则/数据逻辑位于 Core（LinkBoard、LinkPathfinder、BoardSolver、ScoreCalculator、LevelProgress），
    /// 视图/表现位于 Views（NodeSpawner、LinkLineView、GameAudio、GameHudController）。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private float _clearDuration = 0.15f;

        [Header("子系统")]
        [SerializeField] private NodeSpawner _nodeSpawner;
        [SerializeField] private LinkLineView _lineView;
        [SerializeField] private GameAudio _audio;
        [SerializeField] private GameHudController _hud;

        private const int MaxShuffleTries = 20;

        private LinkBoard _board;
        private LinkPathfinder _pathfinder;
        private BoardSolver _solver;
        private SpawnOrderGenerator _orderGen;
        private ScoreCalculator _scoreCalc;
        private LevelProgress _level;
        private BoardSizeConfig _boardSizeConfig;

        private int _mapWidth;
        private int _mapHeight;

        private Node _selectedNode;
        private int _score;
        private bool _isBusy;

        private Coroutine _clearRoutine;
        private Coroutine _spawnRoutine;

        /// <summary>当前本关得分。</summary>
        public int Score => _score;

        /// <summary>当前连击数（连续成功消除的次数）。</summary>
        public int Combo => _scoreCalc != null ? _scoreCalc.Combo : 0;

        /// <summary>积分变化事件，参数为当前总分。</summary>
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            _scoreCalc = new ScoreCalculator();
            _level = new LevelProgress(new PlayerPrefsProgressStore());
            _level.Load();
            _boardSizeConfig = new BoardSizeConfig();

            if (_hud != null)
            {
                _hud.SetLevel(_level.CurrentLevel);
                _hud.SetTotalScore(_level.TotalScore);
            }

            if (_audio != null)
            {
                _audio.Initialize();
            }

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

            if (_board == null || _board.RemainingCount <= 0)
            {
                this.Log("自动连接被忽略：棋盘为空或未初始化。");
                return;
            }

            if (_solver.TryFindLinkablePair(out Vector2Int first, out Vector2Int second))
            {
                Node firstNode = _nodeSpawner.GetNode(first.x, first.y);
                Node secondNode = _nodeSpawner.GetNode(second.x, second.y);
                List<Vector2Int> path = _pathfinder.FindPath(first, second);

                // 找到一对可连接的节点，模拟点击消除
                this.Log($"自动连接: ({first.x},{first.y}) -> ({second.x},{second.y})");
                int turns = Mathf.Max(0, path.Count - 2);
                AddScore(turns);
                ClearSelection();
                _clearRoutine = StartCoroutine(ClearLinkedPair(firstNode, secondNode, path));
                return;
            }

            this.Log("自动连接：当前无解，尝试重排。");
            EnsurePlayable();
        }

        public void StartGame()
        {
            StopActiveRoutines();
            ClearBoard();

            // 棋盘尺寸随关卡动态扩展：第一关 5×6，每 2 关宽高各 +1（封顶 8×12）
            Vector2Int boardSize = _boardSizeConfig.GetBoardSize(_level.CurrentLevel);
            _mapWidth = boardSize.x;
            _mapHeight = boardSize.y;

            int total = _mapWidth * _mapHeight;
            if (total % 2 != 0)
            {
                this.Error($"棋盘格子数 {total} 必须为偶数。");
                return;
            }

            var types = NodeTypeHelper.GeneratePairedTypes(total, _level.GetTypeCountForLevel());

            _board = new LinkBoard(_mapWidth, _mapHeight);
            _board.LoadTypes(types);

            _pathfinder = new LinkPathfinder(_board);
            _solver = new BoardSolver(_board, _pathfinder);
            _orderGen = new SpawnOrderGenerator(_mapWidth, _mapHeight);

            _selectedNode = null;
            _isBusy = false;
            _score = 0;

            if (_nodeSpawner != null)
            {
                _nodeSpawner.Initialize(_mapWidth, _mapHeight);
            }

            if (_hud != null)
            {
                _hud.SetScore(0);
            }

            if (_lineView != null)
            {
                Transform parent = _nodeSpawner != null ? _nodeSpawner.NodeParent : transform;
                _lineView.EnsureGraphic(parent);
            }

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
            var pattern = (SpawnOrderGenerator.SpawnPattern)UnityEngine.Random.Range(
                0, Enum.GetValues(typeof(SpawnOrderGenerator.SpawnPattern)).Length);
            List<Vector2Int> order = _orderGen.Generate(pattern);
            this.Log($"生成特效模式: {pattern}");

            for (int i = 0; i < order.Count; i++)
            {
                Vector2Int pos = order[i];
                int index = pos.x * _mapHeight + pos.y;
                NodeType type = types[index];

                if (_nodeSpawner != null)
                {
                    _nodeSpawner.SpawnNode(OnClickNode, type, pos.x, pos.y);
                }

                // 每创建一个节点暂停一帧，实现边生成边播放
                yield return null;
            }

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

            if (_audio != null)
            {
                _audio.PlayClickSound();
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

            List<Vector2Int> path = _pathfinder.FindPath(_selectedNode.Position, clickNode.Position);
            if (path == null)
            {
                this.Log("无法连接，切换选中。");
                _scoreCalc.ResetCombo();
                SelectNode(clickNode);
                return;
            }

            int turns = Mathf.Max(0, path.Count - 2);
            this.Log($"消除成功，拐点数: {turns}");
            AddScore(turns);
            Node first = _selectedNode;
            Node second = clickNode;
            ClearSelection();
            _clearRoutine = StartCoroutine(ClearLinkedPair(first, second, path));
        }

        private IEnumerator ClearLinkedPair(Node first, Node second, List<Vector2Int> path)
        {
            _isBusy = true;

            if (_lineView != null)
            {
                yield return StartCoroutine(_lineView.PlayLineAnimation(path, _nodeSpawner.Layout));
            }

            if (_audio != null)
            {
                _audio.PlayMatchSound();
            }

            yield return StartCoroutine(PlayClearAnimation(first, second));

            _isBusy = false;
            _clearRoutine = null;

            if (_board.RemainingCount <= 0)
            {
                this.Log("全部消除，进入下一关。");
                SettleLevelScore();
                _level.Advance();
                if (_hud != null)
                {
                    _hud.SetLevel(_level.CurrentLevel);
                }

                yield return new WaitForSeconds(0.35f);
                StartGame();
                yield break;
            }

            EnsurePlayable();
        }

        /// <summary>消除动画：两个节点同时缩小消失后销毁，并同步棋盘模型。</summary>
        private IEnumerator PlayClearAnimation(Node first, Node second)
        {
            first.PlayClearAnimation(_clearDuration, null);
            second.PlayClearAnimation(_clearDuration, null);

            yield return new WaitForSeconds(_clearDuration);

            _board.ClearAt(first.Position);
            _board.ClearAt(second.Position);

            if (_nodeSpawner != null)
            {
                _nodeSpawner.ClearNode(first);
                _nodeSpawner.ClearNode(second);
            }
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

        /// <summary>累加积分并刷新 UI、触发事件。</summary>
        private void AddScore(int turns)
        {
            int points = _scoreCalc.AddScore(turns);
            _score += points;
            if (_hud != null)
            {
                _hud.SetScore(_score);
            }

            OnScoreChanged?.Invoke(_score);
        }

        /// <summary>
        /// 关卡结束结算：将本关得分累加到总分并持久化存储。
        /// </summary>
        private void SettleLevelScore()
        {
            int total = _level.SettleLevelScore(_score);
            if (_hud != null)
            {
                _hud.SetTotalScore(total);
            }

            this.Log($"关卡结算：本关得分 {_score}，累计总分 {total}");
        }

        /// <summary>确保棋盘有解，无解时让求解器重排并刷新视图。</summary>
        private void EnsurePlayable()
        {
            if (_board.RemainingCount <= 0)
            {
                return;
            }

            if (_solver.EnsurePlayable(MaxShuffleTries))
            {
                if (_nodeSpawner != null)
                {
                    _nodeSpawner.SyncBoardToView(_board);
                }

                return;
            }

            this.Warning("重排后仍无解，请重新开始。");
        }

        private void ClearBoard()
        {
            ClearSelection();

            if (_lineView != null)
            {
                _lineView.Hide();
            }

            if (_nodeSpawner != null)
            {
                _nodeSpawner.ClearBoard();
            }

            _board = null;
            _pathfinder = null;
            _solver = null;
            _orderGen = null;
        }

        private void StopActiveRoutines()
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
        }
    }
}