using Nakul.Core;
using UnityEngine;

namespace Nakul.LinkGame
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int _mapWidth;
        [SerializeField] private int _mapHeight;
        [SerializeField] private Node _nodePrefab;
        [SerializeField] private Transform _nodeParent;
        [SerializeField] private int _spacing;
        [SerializeField] private int _xPadding;
        [SerializeField] private int _yPadding;
        [SerializeField] private NodeConfigSO _nodeConfigSo;
        
        // 极简
        // 重复玩
        // Bfs寻路
        // WxApi
        // 动画排版
        // ab包考虑

        private BfsPathfinding _pathfinding;
        private Node[,] _nodeMap;
        private Node _startNode;
        private Node _tagretNode;

        private void Awake()
        {
            var types = NodeTypeHelper.GeneratePairedTypes(_mapWidth * _mapHeight);
            _pathfinding = new BfsPathfinding();
            _nodeMap = new Node[_mapWidth, _mapHeight];
            
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    Node node = Instantiate(_nodePrefab, _nodeParent);
                    node.Position = new Vector2Int(x, y);
                    _nodeMap[x, y] = node;

                    var anchoredPos = new Vector2(_xPadding + ((100 + _spacing) * x), _yPadding + ((100 + _spacing) * y));
                    node.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
                    
                    int index = x * _mapHeight + y;
                    var type = types[index];
                    var sprite = _nodeConfigSo.GetSprite(type);
                    node.Initialize(OnClickNode, type, sprite);
                    
                    node.gameObject.name = $"Node_{x}_{y}_{type}";
                }
            }
            
            _pathfinding.Initialize(_mapWidth, _mapHeight, _nodeMap);
        }

        private void OnClickNode(Node clickNode)
        {
            if (_startNode == null)
            {
                _startNode = clickNode;
            }
            else if (_tagretNode == null)
            {
                _tagretNode = clickNode;
                var path = _pathfinding.FindPath(_startNode, _tagretNode);
                if (path == null)
                {
                    this.Log("寻路失败。");
                    _startNode = null;
                    _tagretNode = null;
                }
                else
                {
                    this.Log("寻路成功");
                    Destroy(_startNode.gameObject);
                    Destroy(_tagretNode.gameObject);
                    _startNode = null;
                    _tagretNode = null;
                }
            }
        }
    }
}