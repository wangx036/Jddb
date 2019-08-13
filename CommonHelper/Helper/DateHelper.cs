using System;

namespace CommonHelper
{
    public static class DateHelper
    {
        /// <summary>
        /// 获取时间戳(毫秒级)
        /// </summary>
        /// <returns></returns>
        public static uint GetTimeStampMilli()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt32(ts.TotalMilliseconds);
        }
        /// <summary>
        /// 获取时间戳(秒级)
        /// </summary>
        /// <returns></returns>
        public static uint GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt32(ts.TotalSeconds);
        }

    }
}