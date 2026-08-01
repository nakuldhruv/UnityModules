using Nakul.Core;

namespace Nakul.Platform
{
    /// <summary>
    /// 平台能力统一入口。
    /// 业务层只依赖此管理器获取各平台能力，具体实现由平台条件编译自动选择。
    /// 用法：PlatformManager.Instance.Share.Share(...) / PlatformManager.Instance.Ad.ShowRewardedVideo(...)
    /// </summary>
    public class PlatformManager : Singleton<PlatformManager>
    {
        private IPlatformBridge _bridge;

        /// <summary>分享能力。</summary>
        public IShareService Share => GetBridge();

        /// <summary>广告能力。</summary>
        public IAdService Ad => GetBridge();

        /// <summary>
        /// 获取桥接实例；未显式初始化时自动使用平台默认实现。
        /// </summary>
        private IPlatformBridge GetBridge()
        {
            if (_bridge == null)
            {
                _bridge = CreateDefaultBridge();
            }
            return _bridge;
        }


        /// <summary>
        /// 初始化平台能力。
        /// 可在游戏启动时显式调用以注入自定义实现；未调用时首次访问会自动使用平台默认实现。
        /// </summary>
        public void Initialize(IPlatformBridge bridge)
        {
            _bridge = bridge ?? new NullPlatformBridge();
        }

        /// <summary>
        /// 获取当前平台的默认实现。
        /// 通过条件编译自动选择，业务层无需关心平台差异。
        /// </summary>
        public static IPlatformBridge CreateDefaultBridge()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // 微信小游戏（WebGL 构建）
            return new WXPlatformBridge();
#else
            // 编辑器 / iOS / Android / 其他平台：暂用空实现兜底，后续按需接入。
            return new NullPlatformBridge();
#endif
        }
    }
}
