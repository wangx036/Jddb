using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CommonHelper
{
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举所有成员名称。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        public static string[] GetNames<T>()
        {
            return Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// 检测枚举是否包含指定成员。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="member">成员名或成员值</param>
        public static bool IsDefined(this Enum value)
        {
            Type type = value.GetType();
            return Enum.IsDefined(type, value);
        }

        /// <summary>
        /// 返回指定枚举类型的指定值的描述。
        /// </summary>
        /// <param name="t">枚举类型</param>
        /// <param name="v">枚举值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            try
            {
                Type type = value.GetType();
                FieldInfo field = type.GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 将枚举转换成字典集合
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDict<T>()
        {

            Dictionary<int, string> resultList = new Dictionary<int, string>();
            Type type = typeof(T);
            var strList = GetNames<T>().ToList();
            foreach (string key in strList)
            {
                string val = Enum.Format(type, Enum.Parse(type, key), "d");
                resultList.Add(int.Parse(val), key);
            }
            return resultList;
        }

        /// <summary>
        /// 将枚举转换成字典集合
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> EnumDescriptionToDict<T>()
        {

            Dictionary<int, string> resultList = new Dictionary<int, string>();
            Type type = typeof(T);
            var strList = GetNames<T>().ToList();
            foreach (string key in strList)
            {
                var e = (Enum)Enum.Parse(type, key);
                string val = Enum.Format(type, Enum.Parse(type, key), "d");
                resultList.Add(int.Parse(val), e.GetDescription());
            }
            return resultList;
        }
    }
}
