using System;
using System.Linq;
using Jddb.Code.Common;
using SqlSugar;

namespace Jddb.Code.Core.Db
{
    public class DbContext
    {
        public DbContext()
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConfigExtensions.Configuration["DbConnection:MySqlConnectionString"],
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices()
                {
                    SqlFuncServices = SqlSugarExternal.SqlFuncExternals()
                }
            });
            //调式代码 用来打印SQL 
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                string s = sql;
                System.Diagnostics.Debug.WriteLine(sql + "\r\n" +
                    Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));

            };
        }
        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作



    }
}
