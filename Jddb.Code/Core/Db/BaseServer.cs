using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jddb.Code.Common;
using Jddb.Code.Model;
using Microsoft.VisualBasic.CompilerServices;

namespace Jddb.Code.Core.Db
{
    public class BaseServer<T> : DbContext where T : class, new()
    {
        #region 添加操作
        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> AddAsync(T parm, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Insertable<T>(parm).ExecuteCommandAsync() : Db.Insertable<T>(parm).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="parm">List<T></param>
        /// <returns></returns>
        public async Task<ApiResult<string>> AddListAsync(List<T> parm, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Insertable<T>(parm).ExecuteCommandAsync() : Db.Insertable<T>(parm).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion

        #region 查询操作
        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="where">Expression<Func<T, bool>></param>
        /// <returns></returns>
        public async Task<ApiResult<T>> GetModelAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = new ApiResult<T>
            {
                statusCode = 200,
                data = Async ? await Db.Queryable<T>().Where(where).FirstAsync() ?? new T() { }
                : Db.Queryable<T>().Where(where).First() ?? new T() { }
            };
            return res;
        }

        /// <summary>
        /// 获得一条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        public async Task<ApiResult<T>> GetModelAsync(string parm, bool Async = true)
        {
            var res = new ApiResult<T>
            {
                statusCode = 200,
                data = Async ? await Db.Queryable<T>().Where(parm).FirstAsync() ?? new T() { }
                : Db.Queryable<T>().Where(parm).First() ?? new T() { }
            };
            return res;
        }

        /// <summary>
		/// 获得列表——分页
		/// </summary>
		/// <param name="parm">PageParm</param>
		/// <returns></returns>
        public async Task<ApiResult<Page<T>>> GetPagesAsync(int pageIndex=1,int pageSize=20, Expression<Func<T, bool>> where=null, bool Async = true)
        {
            var res = new ApiResult<Page<T>>();
            try
            {
                var query = where == null ? Db.Queryable<T>() : Db.Queryable<T>().Where(where);
                res.data = Async ? await query
                        .ToPageAsync(pageIndex, pageSize) : Db.Queryable<T>()
                        .ToPage(pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                res.statusCode = (int)ApiEnum.Error;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
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
        public async Task<ApiResult<Page<T>>> GetPagesAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> where,
            Expression<Func<T, object>> order, Enums.OrderEnum orderEnum, bool Async = true)
        {
            var res = new ApiResult<Page<T>>();
            try
            {
                var query = Db.Queryable<T>()
                        .Where(where)
                    .OrderByIF((int)orderEnum == 1, order, SqlSugar.OrderByType.Asc)
                    .OrderByIF((int)orderEnum == 2, order, SqlSugar.OrderByType.Desc);
                res.data = Async ? await query.ToPageAsync(pageIndex, pageSize) : query.ToPage(pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                res.statusCode = (int)ApiEnum.Error;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
		/// 获得列表
		/// </summary>
		/// <param name="parm">PageParm</param>
		/// <returns></returns>
        public async Task<ApiResult<List<T>>> GetListAsync(Expression<Func<T, bool>> where,
            Expression<Func<T, object>> order, Enums.OrderEnum orderEnum, bool Async = true)
        {
            var res = new ApiResult<List<T>>();
            try
            {
                var query = Db.Queryable<T>()
                        .Where(where)
                    .OrderByIF((int)orderEnum == 1, order, SqlSugar.OrderByType.Asc)
                    .OrderByIF((int)orderEnum == 2, order, SqlSugar.OrderByType.Desc);
                res.data = Async ? await query.ToListAsync() : query.ToList();
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                res.statusCode = (int)ApiEnum.Error;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 获得列表，不需要任何条件
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<T>>> GetListAsync(bool Async = true)
        {
            var res = new ApiResult<List<T>>();
            try
            {
                res.data = Async ? await Db.Queryable<T>().ToListAsync() : Db.Queryable<T>().ToList();
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                res.statusCode = (int)ApiEnum.Error;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion

        #region 修改操作
        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> UpdateAsync(T parm, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Updateable<T>(parm).ExecuteCommandAsync() : Db.Updateable<T>(parm).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="parm">T</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> UpdateAsync(List<T> parm, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Updateable<T>(parm).ExecuteCommandAsync() : Db.Updateable<T>(parm).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 修改一条数据，可用作假删除
        /// </summary>
        /// <param name="columns">修改的列=Expression<Func<T,T>></param>
        /// <param name="where">Expression<Func<T,bool>></param>
        /// <returns></returns>
        public async Task<ApiResult<string>> UpdateAsync(Expression<Func<T, T>> columns,
            Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Updateable<T>().SetColumns(columns).Where(where).ExecuteCommandAsync()
                    : Db.Updateable<T>().SetColumns(columns).Where(where).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion

        #region 删除操作
        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="parm">string</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> DeleteAsync(string parm, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var list = parm.Split(',');
                var dbres = Async ? await Db.Deleteable<T>().In(list.ToArray()).ExecuteCommandAsync() : Db.Deleteable<T>().In(list.ToArray()).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 删除一条或多条数据
        /// </summary>
        /// <param name="where">Expression<Func<T, bool>></param>
        /// <returns></returns>
        public async Task<ApiResult<string>> DeleteAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = new ApiResult<string>() { statusCode = (int)ApiEnum.Error };
            try
            {
                var dbres = Async ? await Db.Deleteable<T>().Where(where).ExecuteCommandAsync() : Db.Deleteable<T>().Where(where).ExecuteCommand();
                res.data = dbres.ToString();
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion

        #region 查询Count
        public async Task<ApiResult<int>> CountAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = new ApiResult<int>() { statusCode = (int)ApiEnum.Error };
            try
            {
                res.data = Async ? await Db.Queryable<T>().CountAsync(where) : Db.Queryable<T>().Count(where);
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion

        #region 是否存在
        public async Task<ApiResult<bool>> IsExistAsync(Expression<Func<T, bool>> where, bool Async = true)
        {
            var res = new ApiResult<bool>() { statusCode = (int)ApiEnum.Error };
            try
            {
                res.data = Async ? await Db.Queryable<T>().AnyAsync(where) : Db.Queryable<T>().Any(where);
                res.statusCode = (int)ApiEnum.Status;
            }
            catch (Exception ex)
            {
                res.message = ex.Message;
                //Logger.Default.ProcessError((int)ApiEnum.Error, ex.Message);
            }
            return res;
        }
        #endregion
    }
}
