using System;

namespace Nakul.Platform
{
    /// <summary>
    /// 广告能力接口。
    /// 业务层只依赖此接口，具体平台实现由 PlatformManager 注入。
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// 激励视频是否已加载完成、可立即展示。
        /// </summary>
        bool IsRewardedVideoReady { get; }

        /// <summary>
        /// 加载激励视频广告。
        /// </summary>
        /// <param name="adUnitId">广告位 ID。</param>
        void LoadRewardedVideo(string adUnitId);

        /// <summary>
        /// 展示激励视频广告。
        /// </summary>
        /// <param name="onComplete">播放结束回调，参数表示是否完整观看（应发放奖励）。</param>
        void ShowRewardedVideo(Action<bool> onComplete);
    }
}
