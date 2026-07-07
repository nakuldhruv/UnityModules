using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityModules
{
    public class DFSPathFinder : MonoBehaviour
    {
        public DFSNode   NodePrefab;
        public Transform NodeContainer;
        public int       XSize;
        public int       YSize;

        private void Awake()
        {
            for (int i = 1; i <= XSize; i++)
            {
                for (int j = 1; j <= YSize; j++)
                {
                    DFSNode go = GameObject.Instantiate(NodePrefab, NodeContainer);
                    go.Initialize(new Vector2Int(i, j));
                    go.name = $"Node{i}_{j}";
                }
            }
        }

        public void Find(DFSNode start, DFSNode dest)
        {
            StartCoroutine(FindCoroutine(start, dest));
        }

        private IEnumerator FindCoroutine(DFSNode start, DFSNode dest)
        {
            _walkableNode.Push(start);
            var node = _walkableNode.Pop();
            
            yield return null;
        }

        private List<DFSNode> GetNeiber(DFSNode node)
        {
            return null;
        }
        
        private Stack<DFSNode>  _walkableNode = new Stack<DFSNode>();
        private HashSet<DFSNode>  _walkedNode = new HashSet<DFSNode>();
        
        private List<Vector2Int> _directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };
    }
}