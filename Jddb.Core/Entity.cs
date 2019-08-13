using System;
using SqlSugar;

namespace Jddb.Core
{
    public abstract class Entity
    {
        /// <summary>
        /// 自增 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
    }
}