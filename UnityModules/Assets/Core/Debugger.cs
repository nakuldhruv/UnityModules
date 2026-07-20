using UnityEngine;

namespace Nakul.Core
{
    public static class Debugger
    {
        public static void Log(this object self, string message)
        {
            Debug.Log($"{nameof(self)}:{message}");
        }
        
        public static void Warning(this object self, string message)
        {
            Debug.LogWarning($"{nameof(self)}:{message}");
        }
        
        public static void Error(this object self, string message)
        {
            Debug.LogError($"{nameof(self)}:{message}");
        }
    }
}