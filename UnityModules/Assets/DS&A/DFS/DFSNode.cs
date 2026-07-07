using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityModules
{
    public class DFSNode : MonoBehaviour
    {
        public Vector2Int Position;
        public DFSNode    ParentNode;
        public bool       IsWalkable;
        public Image      Image;

        public void Initialize(Vector2Int position)
        {
            this.Position = position;
            Image = GetComponent<Image>();
            SetWalkable(true);
        }

        public void SetWalkable(bool isWalkable)
        {
            this.IsWalkable = isWalkable;
            this.Image.color = isWalkable ? Color.green : Color.red;
        }
    }
}