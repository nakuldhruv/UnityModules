using UnityEngine;
using UnityEngine.UI;

namespace Nakul.Platform
{
    public class WxPlatformBridgeTest : MonoBehaviour
    {
        public Button buttonLoadAd;
        public Button buttonShowAd;
        public Button buttonShare;
        public string adUnitId = "adunit-mock123";

        private WXPlatformBridge _wxBridge;

        private void Start()
        {
            _wxBridge = new WXPlatformBridge();
            buttonLoadAd.onClick.AddListener(OnLoadAdClicked);
            buttonShowAd.onClick.AddListener(OnShowAdClicked);
            buttonShare.onClick.AddListener(OnShareClicked);
        }

        private void OnLoadAdClicked()
        {
            _wxBridge.LoadRewardedVideo(adUnitId);
        }

        private void OnShowAdClicked()
        {
            _wxBridge.ShowRewardedVideo(success =>
            {
                if (success)
                {
                    Debug.Log("Reward Sucess");
                }
                else
                {
                    Debug.Log("Reward Failed");
                }
            });
        }

        private void OnShareClicked()
        {
            _wxBridge.Share("Wx分享", "key1=value1&from=test");
        }
    }
}