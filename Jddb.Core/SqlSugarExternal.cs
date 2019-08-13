using System;
using System.Collections.Generic;
using SqlSugar;

namespace Jddb.Core
{
    public static class SqlSugarExternal
    {

        public static List<SqlFuncExternal> SqlFuncExternals()
        {
            var expMethods = new List<SqlFuncExternal>();
            expMethods.Add(new SqlFuncExternal()
            {
                UniqueMethodName = "SqlRound",
                MethodValue = (expInfo, dbType, expContext) =>
                {
                    if (dbType == DbType.MySql || dbType == DbType.SqlServer)
                        return string.Format("Round({0},{1})", expInfo.Args[0].MemberName, expInfo.Args[1].MemberValue);
                    else
                        throw new Exception("未实现");
                }
            });

            return expMethods;
        }

        /// <summary>
        /// round 方法
        /// </summary>
        /// <param name="val"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static double SqlRound(double val,int digit)
        {
            throw new NotSupportedException("Can only be used in expressions");
        }
    }
}