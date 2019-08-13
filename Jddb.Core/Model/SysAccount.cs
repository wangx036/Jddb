using System;
using Newtonsoft.Json;
using SqlSugar;

namespace Jddb.Core.Model
{
    /// <summary>
    /// 用户表
    /// </summary>
    [SugarTable("sys_account")]
    public class SysAccount:Entity
    {

        /// <summary>
        /// 登录名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [JsonIgnore]
        public string LoginPwd { get; set; }

        /// <summary>
        /// salt
        /// </summary>
        [JsonIgnore]
        public string PwdSalt { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }
        /// <summary>
        /// 最多设置任务条数
        /// </summary>
        public int LimitCount { get; set; } = 1;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }=DateTime.Now;
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime? LoginTime { get; set; }
        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }


    }
}