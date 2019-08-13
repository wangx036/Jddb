using System;
using System.Linq;
using SqlSugar;
using CommonHelper;

namespace Jddb.Core
{
    public class DbContext
    {
        public DbContext()
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConfigHelper.Configuration["DbConnection:MySqlConnectionString"],
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

        public SimpleClient<T> GetClient<T>() where T : Entity, new()
        {
            return new SimpleClient<T>(Db);
        }

    }
}
