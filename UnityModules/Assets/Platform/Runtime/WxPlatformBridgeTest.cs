using UnityEngine;
using UnityEngine.UI;

namespace Nakul.Platform
{
    /// <summary>
    /// 平台能力测试脚本。
    /// 演示业务层如何通过 PlatformManager 统一入口使用平台能力，
    /// 而非直接依赖具体平台实现（如 WXPlatformBridge）。
    /// </summary>
    public class WxPlatformBridgeTest : MonoBehaviour
    {
        public Button buttonLoadAd;
        public Button buttonShowAd;
        public Button buttonShare;
        public string adUnitId = "adunit-mock123";

        private IShareService _share;
        private IAdService _ad;

        private void Start()
        {
            // 通过统一入口获取平台能力，业务层不感知具体平台。
            _share = PlatformManager.Instance.Share;
            _ad = PlatformManager.Instance.Ad;

            buttonLoadAd.onClick.AddListener(OnLoadAdClicked);
            buttonShowAd.onClick.AddListener(OnShowAdClicked);
            buttonShare.onClick.AddListener(OnShareClicked);
        }

        private void OnLoadAdClicked()
        {
            _ad.LoadRewardedVideo(adUnitId);
        }

        private void OnShowAdClicked()
        {
            if (!_ad.IsRewardedVideoReady)
            {
                Debug.Log("激励视频尚未就绪，请先加载。");
                return;
            }

            _ad.ShowRewardedVideo(success =>
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
            _share.Share("Wx分享", "key1=value1&from=test");
        }
    }
}
