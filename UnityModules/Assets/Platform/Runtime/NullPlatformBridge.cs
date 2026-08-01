using System;

namespace Nakul.Platform
{
    /// <summary>
    /// 空实现（兜底）。
    /// 用于编辑器、未接入平台或平台初始化失败时，避免业务层空引用崩溃。
    /// </summary>
    public class NullPlatformBridge : IPlatformBridge
    {
        public bool IsRewardedVideoReady => false;

        public void Share(string title, string query)
        {
            // 空实现：无操作。
        }

        public void LoadRewardedVideo(string adUnitId)
        {
            // 空实现：无操作。
        }

        public void ShowRewardedVideo(Action<bool> onComplete)
        {
            // 空实现：直接回调失败，业务层可据此做降级处理。
            onComplete?.Invoke(false);
        }
    }
}
