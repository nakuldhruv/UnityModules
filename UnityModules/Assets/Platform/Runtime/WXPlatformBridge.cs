using System;
using Nakul.Core;
using WeChatWASM;

namespace Nakul.Platform
{
    public class WXPlatformBridge : IPlatformBridge
    {
        private WXRewardedVideoAd _rewardedVideoAd;
        private Action<bool> _onAdComplete;
        
        public void Share(string title, string query)
        {
            WX.ShareAppMessage(new ShareAppMessageOption()
            {
                title = title,
                query = query
            });
        }

        public void LoadRewardedVideo(string adUnitId)
        {
            _rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = adUnitId,
                multiton = false
            });
            
            _rewardedVideoAd.OnLoad(res =>
            {
                this.Log("微信激励视频加载成功。");
            });
            
            _rewardedVideoAd.OnError(res =>
            {
                this.Log("微信激励视频加载/播放失败。");
                HandleAdCallback(false);
            });
            
            _rewardedVideoAd.OnClose(res =>
            {
                bool isSuccess = res != null && res.isEnded || res == null;
                HandleAdCallback(isSuccess);
            });
            
            _rewardedVideoAd.Load();
        }

        public void ShowRewardedVideo(Action<bool> onComplete)
        {
            _onAdComplete = onComplete;
            
            _rewardedVideoAd.Show(res =>
            {
                if (res.errCode != 0)
                {
                    this.Error("广告显示失败。" + res.errMsg);
                    HandleAdCallback(false);
                }
            });
        }

        private void HandleAdCallback(bool success)
        {
            if (_onAdComplete != null)
            {
                var callback = _onAdComplete;
                _onAdComplete = null;
                callback.Invoke(success);
            }
        }
    }
}