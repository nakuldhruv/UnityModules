using System.Collections.Generic;
using UnityEngine;
using Zxs.Event;

namespace Zxs.Guide
{
    public class StartGuide
    {
        public GuideItem Item;
        public StartGuide(GuideItem item) => Item = item;
    }
    
    public class GuideModel : MonoBehaviour
    {
        private List<string> _guideKeys; // 播放过的引导
        private GuideConfig _guideConfig;
        private int _guideIndex;

        private void Awake()
        {
            LoadData();
            TextAsset textAsset = Resources.Load<TextAsset>("GuideConfig");
            _guideConfig = JsonUtility.FromJson<GuideConfig>(textAsset.ToString());

            for (var i = 0; i < _guideConfig.items.Count; i++)
            {
                var guideItem = _guideConfig.items[i];
                if (!_guideKeys.Contains(guideItem.key))
                {
                    _guideIndex = i;
                    break;
                }
            }

            var currentGuide = _guideConfig.items[_guideIndex];
            GameSignalBus.Instance.Fire(new StartGuide(currentGuide));
        }

        private void Update()
        {
            // 更新播放guide
        }

        public void LoadData()
        {
            var rJson = PlayerPrefs.GetString("GuideData");
            _guideConfig = JsonUtility.FromJson<GuideConfig>(rJson);
        }

        public void SaveData()
        {
            PlayerPrefs.SetString("GuideData", JsonUtility.ToJson(_guideKeys));
        }
    }
}