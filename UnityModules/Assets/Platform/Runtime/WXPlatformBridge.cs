using System;
using Nakul.Core;
using WeChatWASM;

namespace Nakul.Platform
{
    /// <summary>
    /// 微信小游戏平台实现。
    /// 实现分享与激励视频广告能力，并维护广告加载/展示状态。
    /// </summary>
    public class WXPlatformBridge : IPlatformBridge
    {
        private WXRewardedVideoAd _rewardedVideoAd;
        private Action<bool> _onAdComplete;
        private bool _isReady;
        private bool _isShowing;

        /// <inheritdoc />
        public bool IsRewardedVideoReady => _isReady && !_isShowing;

        /// <inheritdoc />
        public void Share(string title, string query)
        {
            WX.ShareAppMessage(new ShareAppMessageOption()
            {
                title = title,
                query = query
            });
        }

        /// <inheritdoc />
        public void LoadRewardedVideo(string adUnitId)
        {
            if (_rewardedVideoAd != null)
            {
                // 已创建过广告实例，直接重新加载即可。
                _rewardedVideoAd.Load();
                return;
            }

            _rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = adUnitId,
                multiton = false
            });

            _rewardedVideoAd.OnLoad(res =>
            {
                _isReady = true;
                this.Log("微信激励视频加载成功。");
            });

            _rewardedVideoAd.OnError(res =>
            {
                _isReady = false;
                this.Error("微信激励视频加载/播放失败。");
                HandleAdCallback(false);
            });

            _rewardedVideoAd.OnClose(res =>
            {
                _isReady = false;
                _isShowing = false;
                bool isSuccess = res != null && res.isEnded || res == null;
                HandleAdCallback(isSuccess);
            });

            _rewardedVideoAd.Load();
        }

        /// <inheritdoc />
        public void ShowRewardedVideo(Action<bool> onComplete)
        {
            if (_rewardedVideoAd == null)
            {
                this.Error("激励视频尚未加载，请先调用 LoadRewardedVideo。");
                onComplete?.Invoke(false);
                return;
            }

            if (_isShowing)
            {
                this.Warning("激励视频正在展示中，忽略重复调用。");
                return;
            }

            _onAdComplete = onComplete;
            _isShowing = true;

            _rewardedVideoAd.Show(res =>
            {
                if (res.errCode != 0)
                {
                    _isShowing = false;
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
