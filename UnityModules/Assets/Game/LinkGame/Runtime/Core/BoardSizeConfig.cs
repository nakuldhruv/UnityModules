using UnityEngine;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 棋盘尺寸配置：随关卡动态扩展的纯逻辑类。
    /// 第一关从 5×6 起步，每 <see cref="LevelPerSizeStep"/> 关宽高各 +1，
    /// 形成 5×6 → 6×7 → 7×8 → 8×9 → … 的矩形渐变节奏；
    /// 宽度封顶 <see cref="MaxWidth"/>，高度封顶 <see cref="MaxHeight"/>。
    /// 宽（奇）× 高（偶）保证格子总数恒为偶数，无需额外修正。
    /// </summary>
    public sealed class BoardSizeConfig
    {
        public const int MinWidth = 5;
        public const int MinHeight = 6;
        public const int MaxWidth = 8;
        public const int MaxHeight = 12;
        public const int LevelPerSizeStep = 2;

        /// <summary>根据当前关卡返回棋盘宽高。</summary>
        public Vector2Int GetBoardSize(int level)
        {
            int step = Mathf.Max(0, (level - 1) / LevelPerSizeStep);
            int width = Mathf.Min(MinWidth + step, MaxWidth);
            int height = Mathf.Min(MinHeight + step, MaxHeight);
            return new Vector2Int(width, height);
        }
    }
}