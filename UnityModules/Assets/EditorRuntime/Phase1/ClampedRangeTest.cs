using UnityEngine;

namespace Nakul.Editor
{
    public class ClampedRangeTest : MonoBehaviour
    {
        [ClampedRange(0, 100)]
        public float floatRangeAttr;
        [ClampedRange(0, 100)]
        public int intRangeAttr;
        
        [Space]
        [Header("对比：系统默认 Range")]
        [Range(0, 100)]
        public float defaultRange;
    }
}