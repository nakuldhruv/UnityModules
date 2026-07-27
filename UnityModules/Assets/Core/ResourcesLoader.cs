using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nakul.Core
{
    public class ResourcesLoader : IAssetLoader
    {
        public T LoadAsset<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public void LoadAssetAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            request.completed += _ => onComplete?.Invoke((T)request.asset);
        }

        public void UnloadAsset(Object asset)
        {
            Resources.UnloadAsset(asset);
        }
    }
}