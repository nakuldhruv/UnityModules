using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nakul.LinkGame
{
    public class Node : MonoBehaviour
    {
        public Vector2Int Position { get; set; }
        public NodeType Type { get; set; }
        public Node Parent { get; set; }
        public bool IsWalkable => Type == NodeType.None; // 起点终点不需要判断
        
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;

        private Action<Node> _onClickAction;

        public void Initialize(Action<Node> onClickAction, NodeType nodeType, Sprite nodeSprite)
        {
            _onClickAction = onClickAction;
            _button.onClick.AddListener(OnClick);
            Type = nodeType;
            _image.sprite = nodeSprite;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            _onClickAction?.Invoke(this);
        }
    }
}