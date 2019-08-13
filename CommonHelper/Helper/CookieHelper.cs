using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace CommonHelper
{
    #region Session 帮助类

    /// <summary>
    /// Session 帮助类
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// HttpContext 对象
        /// </summary>
        private static HttpContext MyContext => MyHttpContext.HttpContext;
        /// <summary>
        /// 设置 Session
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set( string key, object value)
        {
            MyContext.Session.SetString(key, JsonConvert.SerializeObject(value));
        }
        /// <summary>
        /// 获取 Session
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get( string key)
        {
            return Get<string>(key);
        }
        /// <summary>
        /// 获取 Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>( string key)
        {
            var value = MyContext.Session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 删除 session
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            MyContext.Session.Remove(key);
        }
    }

    #endregion

    #region Cookie 帮助类

    /// <summary>
    /// Cookie 帮助类
    /// </summary>
    public static class CookieHelper
    {
        /// <summary>
        /// HttpContext 对象
        /// </summary>
        private static HttpContext MyContext => MyHttpContext.HttpContext;
        /// <summary>
        /// 设置 Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value, int? expires = null)
        {
            if (expires == null)
                MyContext.Response.Cookies.Append(key, value);
            else
                MyContext.Response.Cookies.Append(key, value, new CookieOptions()
                {
                    Expires = DateTime.Now.AddMinutes(Convert.ToDouble(expires))
                });
        }
        /// <summary>
        /// 获取 Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            var value = MyContext.Request.Cookies[key];
            return value ?? "";
        }
        /// <summary>
        /// 获取 Cookie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            var value = MyContext.Request.Cookies[key];

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 删除 Cookie
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            MyContext.Response.Cookies.Delete(key);
        }
    }


    #endregion


}