using System.ComponentModel;
using System.Threading.Tasks;

namespace CommonHelper
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
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 数据集
        /// </summary>
        public T Data { get; set; }
    }

    public class ApiResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// returnmethod 返回方法
    /// </summary>
    public static class RM
    {
        #region 返回结果封装

        public static ApiResult<T> Success<T>(string msg = "操作成功", T data=default(T)) 
        {
            return new ApiResult<T>()
            {
                IsSuccess = true,
                StatusCode = (int)StatusEnum.Success,
                Message = msg,
                Data = data
            };
        }
        public static ApiResult<T> Error<T>(string msg = "操作失败", T data = default(T), StatusEnum status = StatusEnum.Error)
        {
            return new ApiResult<T>()
            {
                IsSuccess = false,
                StatusCode = (int)status,
                Message = msg,
                Data = data
            };
        }

        public static async Task<ApiResult<T>> SuccessTask<T>(string msg = "操作成功", T data = default(T))
        {
            return await Task.Run(() => new ApiResult<T>()
            {
                IsSuccess = true,
                StatusCode = (int)StatusEnum.Success,
                Message = msg,
                Data = data
            });
        }

        public static async Task<ApiResult<T>> ErrorTask<T>(string msg = "操作失败", T data = default(T), StatusEnum status = StatusEnum.Error)
        {
            return await Task.Run(() => new ApiResult<T>()
            {
                IsSuccess = false,
                StatusCode = (int)status,
                Message = msg,
                Data = data
            });
        }

        #endregion
    }

    public enum StatusEnum
    {
        /// <summary>
        /// 请求(或处理)成功
        /// </summary>
        [Description("请求(或处理)成功")]
        Success = 200, //请求(或处理)成功

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
