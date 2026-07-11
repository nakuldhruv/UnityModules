using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityModules
{
    public class LinkGameMapNode : MonoBehaviour
    {
        public int X => position.x;
        public int Y => position.y;

        public Vector2Int position;
        public LinkGameMapNodeType nodeType;
        public Action<LinkGameMapNode> onClickAction;

        [SerializeField] private Image iconImage;

        public void SetPosition(int x, int y)
        {
            this.position = new Vector2Int(x, y);
        }

        public void SetNodeType(LinkGameMapNodeType mapNodeType)
        {
            nodeType = mapNodeType;
            iconImage.color = mapNodeType.GetColor();
        }

        public void OnClick()
        {
            onClickAction?.Invoke(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            iconImage.color = nodeType.GetColor();
        }
#endif
    }
}