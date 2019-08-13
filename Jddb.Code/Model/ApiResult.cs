using System.ComponentModel;

namespace Jddb.Code.Model
{
    /// <summary>
    /// API 返回JSON字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; } = true;
        /// <summary>
        /// 信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int statusCode { get; set; } = 200;
        /// <summary>
        /// 数据集
        /// </summary>
        public T data { get; set; }
    }

    public enum ApiEnum
    {
        /// <summary>
        /// 请求(或处理)成功
        /// </summary>
        [Description("请求(或处理)成功")]
        Status = 200, //请求(或处理)成功

        /// <summary>
        /// 内部请求出错
        /// </summary>
        [Description("内部请求出错")]
        Error = 500, //内部请求出错

        /// <summary>
        /// 未授权标识
        /// </summary>
        [Description("未授权标识")]
        Unauthorized = 401,//未授权标识

        /// <summary>
        /// 请求参数不完整或不正确
        /// </summary>
        [Description("请求参数不完整或不正确")]
        ParameterError = 400,//请求参数不完整或不正确

        /// <summary>
        /// 请求TOKEN失效
        /// </summary>
        [Description("请求TOKEN失效")]
        TokenInvalid = 403,//请求TOKEN失效

        /// <summary>
        /// HTTP请求类型不合法
        /// </summary>
        [Description("HTTP请求类型不合法")]
        HttpMehtodError = 405,//HTTP请求类型不合法

        /// <summary>
        /// HTTP请求不合法,请求参数可能被篡改
        /// </summary>
        [Description("HTTP请求不合法,请求参数可能被篡改")]
        HttpRequestError = 406,//HTTP请求不合法

        /// <summary>
        /// 该URL已经失效
        /// </summary>
        [Description("该URL已经失效")]
        URLExpireError = 407,//HTTP请求不合法
    }
}
