#if UNITY_EDITOR
using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Nakul.Core
{
    public class EditorLoader : IAssetLoader
    {
        public T LoadAsset<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public void LoadAssetAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            T asset = LoadAsset<T>(path);
            onComplete?.Invoke(asset);
        }

        public void UnloadAsset(Object asset)
        {
        }
    }
}
#endif