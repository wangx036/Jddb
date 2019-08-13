using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonHelper;
using CommonHelper.Filter;
using Jddb.Core.Extend;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Jddb.Web.Controllers
{

    [Route("[controller]")]
    public class BaseController : Controller
    {

        /// <summary>
        /// 返回登录人的信息
        /// </summary>
        /// <returns></returns>
        protected TokenModel LoginUser()
        {
            var tm = new TokenModel();
            //检测是否包含'Authorization'请求头，如果不包含则直接放行
            if (HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var tokenHeader = HttpContext.Request.Headers["Authorization"];
                tokenHeader = tokenHeader.ToString().Substring("Bearer ".Length).Trim();

                tm = JwtHelper.SerializeJWT(tokenHeader);
            }
            
            return tm;
        }



        #region 返回结果封装

        public static ApiResult<T> Success<T>(string msg = "操作成功", T data = default(T))
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
}