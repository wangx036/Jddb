using Jddb.Core;
using Jddb.Core.Extend;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;
using CommonHelper;
using Microsoft.VisualBasic.CompilerServices;

namespace Jddb.Service
{
    public class BaseServer<T> : DbContext where T : class, new()
    {
        #region 添加操作
        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public async Task<T> AddAsync(T parm, bool Async = true)
        {
            var dbres = Async ? await Db.Insertable<T>(parm).ExecuteReturnEntityAsync() : Db.Insertable<T>(parm).ExecuteReturnEntity();
            return dbres;
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns>受影响行数</returns>
        public async Task<int> AddListAsync(List<T> parm, bool Async = true)
        {
            var dbres = Async ? await Db.Insertable<T>(parm).ExecuteCommandAsync() : Db.Insertable<T>(parm).ExecuteCommand();
            return dbres;
        }
        #endregion

        #region 查询操作
        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="where">Expression<Func<T, bool>></param>
        /// <returns></returns>
        public async Task<T> GetModelAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = Async
                ? await Db.Queryable<T>().Where(where).FirstAsync() ?? new T() { }
                : Db.Queryable<T>().Where(where).First() ?? new T() { };
            return res;
        }

        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        public async Task<T> GetModelAsync(string parm, bool Async = true)
        {
            var res = Async ? await Db.Queryable<T>().Where(parm).FirstAsync() ?? new T() { }
                : Db.Queryable<T>().Where(parm).First() ?? new T() { };
            return res;
        }

        /// <summary>
		/// 获得列表——分页
		/// </summary>
		/// <param name="parm">PageParm</param>
		/// <returns></returns>
        public async Task<Page<T>> GetPagesAsync(int pageIndex,int pageSize, bool Async = true)
        {
            var res = Async ? await Db.Queryable<T>()
                .ToPageAsync(pageIndex, pageSize) : Db.Queryable<T>()
                .ToPage(pageIndex, pageSize);
            return res;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="parm">分页参数</param>
        /// <param name="where">条件</param>
        /// <param name="order">排序值</param>
        /// <param name="orderEnum">排序方式OrderByType</param>
        /// <returns></returns>
        public async Task<Page<T>> GetPagesAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> where,
            Expression<Func<T, object>> order, OrderByEnmu orderEnum, bool Async = true)
        {
            var query = Db.Queryable<T>()
                .Where(where)
                .OrderByIF((int)orderEnum == 1, order, SqlSugar.OrderByType.Asc)
                .OrderByIF((int)orderEnum == 2, order, SqlSugar.OrderByType.Desc);
            var res = Async ? await query.ToPageAsync(pageIndex, pageSize) : query.ToPage(pageIndex, pageSize);
            return res;
        }

        /// <summary>
		/// 获得列表
		/// </summary>
		/// <param name="parm">PageParm</param>
		/// <returns></returns>
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where,
            Expression<Func<T, object>> order, OrderByEnmu orderEnum, bool Async = true)
        {
            var query = Db.Queryable<T>()
                .Where(where)
                .OrderByIF((int)orderEnum == 1, order, SqlSugar.OrderByType.Asc)
                .OrderByIF((int)orderEnum == 2, order, SqlSugar.OrderByType.Desc);
            var res = Async ? await query.ToListAsync() : query.ToList();
            return res;
        }

        /// <summary>
        /// 获得列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where =null,bool Async = true)
        {
            var quary = where==null? Db.Queryable<T>(): Db.Queryable<T>().Where(where);
            var res = Async ? await quary.ToListAsync() : quary.ToList();
            return res;
        }
        #endregion

        #region 修改操作
        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateAsync(T parm, bool Async = true)
        {
            var res = Async ? await Db.Updateable<T>(parm).ExecuteCommandHasChangeAsync() : Db.Updateable<T>(parm).ExecuteCommandHasChange();
            return res;
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns>受影响行数</returns>
        public async Task<int> UpdateAsync(List<T> parm, bool Async = true)
        {
            var res = Async ? await Db.Updateable<T>(parm).ExecuteCommandAsync() : Db.Updateable<T>(parm).ExecuteCommand();
            return res;
        }

        /// <summary>
        /// 修改一条数据，可用作假删除
        /// </summary>
        /// <param name="columns">修改的列=Expression<Func<T,T>></param>
        /// <param name="where">Expression<Func<T,bool>></param>
        /// <returns>受影响行数</returns>
        public async Task<int> UpdateAsync(Expression<Func<T, T>> columns,
            Expression<Func<T, bool>> where, bool Async = true)
        {
            var res =  Async ? await Db.Updateable<T>().SetColumns(columns).Where(where).ExecuteCommandAsync()
                : Db.Updateable<T>().SetColumns(columns).Where(where).ExecuteCommand();
            return res;
        }
        #endregion

        #region 删除操作
        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns>受影响行数</returns>
        public async Task<int> DeleteAsync(string parm, bool Async = true)
        {
            var list = parm.Split(",");
            var res = Async ? await Db.Deleteable<T>().In(list).ExecuteCommandAsync() : Db.Deleteable<T>().In(list).ExecuteCommand();
            return res;
        }

        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="where">Expression<Func<T, bool>></param>
        /// <returns>受影响行数</returns>
        public async Task<int> DeleteAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = Async ? await Db.Deleteable<T>().Where(where).ExecuteCommandAsync() : Db.Deleteable<T>().Where(where).ExecuteCommand();
            return res;
        }
        #endregion

        #region 查询Count
        public async Task<int> CountAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = Async ? await Db.Queryable<T>().CountAsync(where) : Db.Queryable<T>().Count(where);
            return res;
        }
        #endregion

        #region 是否存在
        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = Async ? await Db.Queryable<T>().AnyAsync(where) : Db.Queryable<T>().Any(where);
            return res;
        }
        #endregion
    }
}
