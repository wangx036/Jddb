using System;
using System.Collections.Generic;
using System.Text;

namespace Jddb.Code.Common
{
    public static class TimeHelper
    {
        /// <summary>
        /// 时间戳转换为日期
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timeStamp)
        {
            var start = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return timeStamp.ToString().Length == 13
                ? start.AddMilliseconds(timeStamp)
                : start.AddSeconds(timeStamp);
        }

        /// <summary>
        /// 日期转换为时间戳(10位，秒级)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime time)
        {
            DateTime Jan1st1970 = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(time - Jan1st1970).TotalMilliseconds;
        }
        /// <summary>
        /// 日期转换为时间戳(13位，毫秒级)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToTimeStampMs(this DateTime time)
        {
            DateTime Jan1st1970 = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(time - Jan1st1970).TotalMilliseconds;
        }

        /// <summary>
		/// 获取以0点0分0秒开始的日期
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static DateTime GetStartDateTime(DateTime d)
        {
            if (d.Hour != 0)
            {
                var year = d.Year;
                var month = d.Month;
                var day = d.Day;
                var hour = "0";
                var minute = "0";
                var second = "0";
                d = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", year, month, day, hour, minute, second));
            }
            return d;
        }

        /// <summary>
        /// 获取以23点59分59秒结束的日期
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime GetEndDateTime(DateTime d)
        {
            if (d.Hour != 23)
            {
                var year = d.Year;
                var month = d.Month;
                var day = d.Day;
                var hour = "23";
                var minute = "59";
                var second = "59";
                d = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", year, month, day, hour, minute, second));
            }
            return d;
        }
    }
}