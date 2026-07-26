using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nakul.LinkGame
{
    public static class NodeTypeHelper
    {
        private static readonly NodeType[] ValidNodeTypes = CacheValidNodeTypes();

        private static NodeType[] CacheValidNodeTypes()
        {
            var allTypes = (NodeType[])Enum.GetValues(typeof(NodeType));
            List<NodeType> list = new List<NodeType>();

            foreach (var type in allTypes)
            {
                if (type != NodeType.None)
                {
                    list.Add(type);
                }
            }

            return list.ToArray();
        }

        public static NodeType GetRandomType()
        {
            int randomIndex = UnityEngine.Random.Range(0, ValidNodeTypes.Length);
            return ValidNodeTypes[randomIndex];
        }

        public static List<NodeType> GeneratePairedTypes(int totalCount)
        {
            if (totalCount % 2 != 0)
            {
                Debug.LogError($"[NodeTypeHelper] 方块总数 {totalCount} 不是偶数，连连看无法完全消除！");
            }

            List<NodeType> typesList = new List<NodeType>(totalCount);
            int pairCount = totalCount / 2;

            for (int i = 0; i < pairCount; i++)
            {
                NodeType randomType = GetRandomType();
                typesList.Add(randomType);
                typesList.Add(randomType);
            }

            Shuffle(typesList);

            return typesList;
        }

        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}