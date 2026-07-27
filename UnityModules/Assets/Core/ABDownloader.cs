using UnityEngine;

namespace Nakul.Core
{
    public class ABDownloader
    {
        private readonly string        _serverUrl;
        private readonly string        _saveBasePath;
        private readonly MonoBehaviour _runner;

        public ABDownloader(string serverUrl, MonoBehaviour runner)
        {
            _serverUrl = serverUrl.TrimEnd('/');
            _saveBasePath = Application.persistentDataPath;
            _runner = runner;
        }
    }
}