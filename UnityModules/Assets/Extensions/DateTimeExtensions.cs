namespace UnityModules
{
    public static class DateTimeExtensions
    {
        public static long ToMidnightUnixTimeSeconds(this System.DateTime dateTime)
        {
            var midnight = new System.DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
            var epoch = new System.DateTime(1970, 1, 1, 0, 0, 0);
            return (long)(midnight - epoch).TotalSeconds;
        }

        public static long ToUnixTimeSeconds(this System.DateTime dateTime)
        {
            var epoch = new System.DateTime(1970, 1, 1, 0, 0, 0);
            var seconds = (long)(dateTime - epoch).TotalSeconds;
            return seconds;
        }
    }

/*
 * Unix 时间戳（Unix timestamp）定义为 从 1970 年 1 月 1 日 00:00:00 UTC 开始经过的秒数（不含闰秒）。
 * 这个起始时间正是 Unix 系统的纪元（Epoch），最初由早期的 Unix 开发者选定，并沿用至今，成为很多计算机系统的时间表示基准。
 */
}