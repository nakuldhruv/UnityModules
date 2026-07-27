using System;
using Object = UnityEngine.Object;

namespace Nakul.Core
{
    public interface IAssetLoader
    {
        T LoadAsset<T>(string path) where T : Object;
        
        void LoadAssetAsync<T>(string path, Action<T> onComplete) where T : Object;
        
        void UnloadAsset(Object asset);
    }
}