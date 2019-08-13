using System;
using SqlSugar;

namespace Jddb.Code.Model
{
    /// <summary>
    /// 用户表
    /// </summary>
    [SugarTable("sys_account")]
    public class SysAccount
    {
        /// <summary>
        /// Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string LoginPwd { get; set; }

        /// <summary>
        /// salt
        /// </summary>
        public string PawSalt { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }=DateTime.Now;

        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }


    }
}