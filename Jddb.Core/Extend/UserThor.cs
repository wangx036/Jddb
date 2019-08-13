using System;
using System.Collections.Generic;
using System.Text;
using CommonHelper;
using Newtonsoft.Json;

namespace Jddb.Core.Extend
{
    /// <summary>
    /// 用户thor信息
    /// </summary>
    public class UserThor
    {
        /// <summary>
        /// 登录信息cookie
        /// </summary>
        [JsonIgnore]
        public string Thor => ThorEncrypt.DESDecrypt();
        /// <summary>
        /// 登录信息cookie (加密)
        /// </summary>
        public string ThorEncrypt { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Mail { get; set; }
        /// <summary>
        /// 状态（true有效，false失效）
        /// </summary>
        public bool Status { get; set; } = true;
    }

    /// <summary>
    /// 已登录用户信息
    /// </summary>
    public class LoginInfo
    {

        /// <summary>
        /// 用户id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

    }
}
