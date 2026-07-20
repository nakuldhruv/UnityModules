using System;
using Nakul.Core;
using WeChatWASM;

namespace Nakul.Platform
{
    public class WXBridge : IPlatformBridge
    {
        private WXRewardedVideoAd _rewardedVideoAd;
        private string _currentAdUnitId;
        private bool _isAdLoaded;
        
        public void Share(string title, string query)
        {
            ShareAppMessageOption option = new ShareAppMessageOption();
            option.title = title;
            option.query = query;
            WX.ShareAppMessage(option);
            this.Log("真机调试分享。");
        }

        public void LoadRewardedVideo(string adUnitId)
        {
            _currentAdUnitId = adUnitId;
            _rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = adUnitId,
            });
        }

        public void ShowRewardedVideo(Action<bool> onComplete)
        {
            throw new NotImplementedException();
        }
    }
}