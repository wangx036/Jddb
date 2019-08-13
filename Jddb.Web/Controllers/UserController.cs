using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonHelper;
using CommonHelper.Cache;
using CommonHelper.Filter;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Jddb.Web.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [JwtAuthorize(Roles = "Admin")]
    public class UserController : BaseController
    {
        private ICacheService _cacheService;
        private WebServer _webServer;
        private BaseServer<SysAccount> _sysAccountServer;

        public UserController(ICacheService cacheService, WebServer webServer,BaseServer<SysAccount> sysAccountServer)
        {
            _cacheService = cacheService;
            _webServer = webServer;
            _sysAccountServer = sysAccountServer;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [AllowAnonymous]
        public async Task<ApiResult<SysAccount>> Add(SysAccount account)
        {
            if (await _sysAccountServer.IsExistAsync(o => o.Mobile == account.Mobile))
                return await ErrorTask<SysAccount>("手机号已存在，请重新输入");
            if (await _sysAccountServer.IsExistAsync(o => o.UserName == account.UserName))
                return await ErrorTask<SysAccount>("用户名已存在，请重新输入");

            account.LoginPwd = EncryptHelper.MD5Encrypt(account.LoginPwd + account.PwdSalt);

            var resModel = await _sysAccountServer.AddAsync(account);
            return await SuccessTask(data: resModel);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userName">登录名</param>
        /// <param name="loginPwd">密码</param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ApiResult<string>> Login(string userName, string loginPwd)
        {
            var account = await _sysAccountServer.GetModelAsync(o => o.UserName == userName);
            var pwdEncrypt = EncryptHelper.MD5Encrypt(loginPwd + account.PwdSalt);
            if (pwdEncrypt != account.LoginPwd)
                return await ErrorTask<string>("用户名或密码错误");

            // 更新登录时间
            account.LastLoginTime = account.LoginTime;
            account.LoginTime=DateTime.Now;
            _sysAccountServer.UpdateAsync(
                o => new SysAccount() {LastLoginTime = account.LastLoginTime, LoginTime = account.LoginTime},
                o => o.Id == account.Id);

            var identity = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Sid,account.Id.ToString()),
                    new Claim(ClaimTypes.GroupSid,account.UserType.ToString()),
                    new Claim(ClaimTypes.Name,account.UserName),
                    new Claim(ClaimTypes.UserData,account.LoginTime.ToString())
                }, CookieAuthenticationDefaults.AuthenticationScheme)
            );
            //如果保存用户类型是Session，则默认设置cookie退出浏览器 清空
            if (ConfigHelper.Configuration[AppKeys.LOGINSAVEUSER] == "Session")
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, identity, new AuthenticationProperties
                {
                    AllowRefresh = false
                });
            }
            else
            {
                //根据配置保存浏览器用户信息，小时单位
                var hours = int.Parse(ConfigHelper.Configuration[AppKeys.LOGINCOOKIEEXPIRES]);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, identity, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddHours(hours),
                    IsPersistent = true,
                    AllowRefresh = false
                });
            }
            var token = JwtHelper.IssueJWT(new TokenModel()
            {
                Id = account.Id,
                Uid = account.Id.ToString(),
                UserName = account.UserName,
                UserType = account.UserType,
                Role = "Admin",
                TokenType = "Web"
            });

            return await SuccessTask(data: token);
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<ApiResult<bool>> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return await SuccessTask(data: true);
        }

        /// <summary>
        /// 设置用户thor
        /// </summary>
        /// <param name="thor"></param>
        /// <returns></returns>
        [HttpPost("setThor")]
        public async Task<ApiResult<UserThor>> SetUserThor(string thor)
        {
            var user = LoginUser();
            var key = MyKeys.RedisUserInfo(user.Id);
            var model = _webServer.GetUserInfo(thor);
            if (string.IsNullOrWhiteSpace(model.NickName))
                return await ErrorTask<UserThor>("Thor无效");

            // 用户信息
            var account = await _sysAccountServer.GetModelAsync(o => o.Id == user.Id);
            model.Mail = account.Mail;

            RedisHelper.Set(key, model);
            return await SuccessTask(data:model);
        }




    }
}