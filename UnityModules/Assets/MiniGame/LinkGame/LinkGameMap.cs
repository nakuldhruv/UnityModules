using System.Collections.Generic;
using UnityEngine;

namespace UnityModules
{
    public class LinkGameMap : MonoBehaviour
    {
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private LinkGameMapNode nodePrefab;
        [SerializeField] private Transform nodeParent;
        
        private readonly List<LinkGameMapNode> _nodeInstances = new List<LinkGameMapNode>();

        public void CreateMap()
        {
            if (mapWidth * mapHeight % 2 != 0)
            {
                Debug.LogError("配置的地图节点数量不能为奇数。");
                return;
            }
                
            for (int i = 1; i <= mapWidth; i++)
            {
                for (int j = 1; j <= mapHeight; j++)
                {
                    LinkGameMapNode nodeInstance = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, nodeParent);
                    nodeInstance.gameObject.name = $"LinkGameMapNode_{i}_{j}";
                    _nodeInstances.Add(nodeInstance);
                }
            }
            
            // 洗牌获取值
        }
    }
}