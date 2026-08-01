namespace Nakul.Platform
{
    /// <summary>
    /// 分享能力接口。
    /// 业务层只依赖此接口，具体平台实现由 PlatformManager 注入。
    /// </summary>
    public interface IShareService
    {
        /// <summary>
        /// 发起分享。
        /// </summary>
        /// <param name="title">分享标题。</param>
        /// <param name="query">分享携带的查询参数（如 key1=value1&from=test）。</param>
        void Share(string title, string query);
    }
}
