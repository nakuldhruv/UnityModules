using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nakul.Core
{
    public class ABInfo
    {
        public AssetBundle Bundle;
        public int         RefCount;

        public ABInfo(AssetBundle bundle)
        {
            Bundle = bundle;
            RefCount = 0;
        }
    }
    
    public class ABLoader : IAssetLoader
    {
        private readonly string _persistentPath = Application.persistentDataPath;
        private readonly string _streamingPath = Application.streamingAssetsPath;
        
        private readonly Dictionary<string, ABInfo> _loadedBundles = new Dictionary<string, ABInfo>();
        
        private AssetBundleManifest _manifest;
        
        private readonly MonoBehaviour _coroutineRunner;

        public ABLoader(string mainManifestBundleName, MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            InitManifest(mainManifestBundleName.ToLower());
        }

        private string GetRealFilePath(string abName)
        {
            string hotPath = Path.Combine(_persistentPath, abName);
            if (File.Exists(hotPath))
            {
                return hotPath;
            }
            
            return Path.Combine(_streamingPath, abName);
        }

        private void InitManifest(string abName)
        {
            string realPath = GetRealFilePath(abName);
            if (!File.Exists(realPath))
            {
                Debug.LogError($"Could not find AssetBundle at path {realPath}");
                return;
            }
            
            AssetBundle assetBundle = AssetBundle.LoadFromFile(realPath);
            _manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        private AssetBundle LoadBundleWithDependencies(string abName)
        {
            abName = abName.ToLower();
            if (_manifest != null)
            {
                string[] dependencies = _manifest.GetAllDependencies(abName);
                foreach (string dependency in dependencies)
                {
                    LoadBundleWithDependencies(dependency);
                }
            }

            if (_loadedBundles.TryGetValue(abName, out ABInfo abInfo))
            {
                abInfo.RefCount++;
                return abInfo.Bundle;
            }
            
            string realPath = GetRealFilePath(abName);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(realPath);
            if (assetBundle == null)
            {
                Debug.LogError($"Could not find AssetBundle at path {realPath}");
                return null;
            }
            
            _loadedBundles.Add(abName, new ABInfo(assetBundle));
            return assetBundle;
        }

        public void UnloadBundle(string abName, bool unloadAllLoadedObjects = false)
        {
            abName = abName.ToLower();
            if (_manifest != null)
            {
                string[] dependencies = _manifest.GetAllDependencies(abName);
                foreach (string dependency in dependencies)
                {
                    UnloadBundle(dependency, unloadAllLoadedObjects);
                }
            }

            if (_loadedBundles.TryGetValue(abName, out ABInfo abInfo))
            {
                abInfo.RefCount--;
                if (abInfo.RefCount <= 0)
                {
                    abInfo.Bundle.Unload(unloadAllLoadedObjects);
                    _loadedBundles.Remove(abName);
                }
            }
        }
        
        public T LoadAsset<T>(string path) where T : Object
        {
            throw new NotImplementedException();
        }

        public void LoadAssetAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            throw new NotImplementedException();
        }

        public void UnloadAsset(Object asset)
        {
            throw new NotImplementedException();
        }
    }
}