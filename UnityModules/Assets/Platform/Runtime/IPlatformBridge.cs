namespace Nakul.Platform
{
    /// <summary>
    /// 平台桥接总接口。
    /// 聚合各能力接口，供具体平台实现（如 WXPlatformBridge）一次性实现全部能力。
    /// 业务层应优先依赖细粒度接口（IShareService / IAdService），而非此总接口。
    /// </summary>
    public interface IPlatformBridge : IShareService, IAdService
    {
    }
}
