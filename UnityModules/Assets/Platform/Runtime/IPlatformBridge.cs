namespace Nakul.Platform
{
    public interface IPlatformBridge
    {
        void Share(string title, string query);
        
        void LoadRewardedVideo(string adUnitId);

        void ShowRewardedVideo(System.Action<bool> onComplete);
    }
}