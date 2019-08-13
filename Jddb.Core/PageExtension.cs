using System.Threading.Tasks;
using CommonHelper;
using SqlSugar;

namespace Jddb.Core
{
    public static class PageExtension
    {
        /// <summary>
        /// 读取列表QueryableExtension
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isOrderBy"></param>
        /// <returns></returns>
        public static async Task<Page<T>> ToPageAsync<T>(this ISugarQueryable<T> query,
            int pageIndex,
            int pageSize,
            bool isOrderBy = false)
        {
            RefAsync<int> totalItems = 0;
            var page = new Page<T>();
            page.Items = await query.ToPageListAsync(pageIndex, pageSize, totalItems);
            page.CurrentPage = pageIndex;
            page.PageSize = pageSize;
            page.TotalItems = totalItems;
            return page;
        }

        /// <summary>
        /// 读取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isOrderBy"></param>
        /// <returns></returns>
        public static Page<T> ToPage<T>(this ISugarQueryable<T> query,
            int pageIndex,
            int pageSize,
            bool isOrderBy = false)
        {
            var page = new Page<T>();
            var totalItems = 0;
            page.Items = query.ToPageList(pageIndex, pageSize, ref totalItems);
            page.CurrentPage = pageIndex;
            page.PageSize = pageSize;
            page.TotalItems = totalItems;
            return page;
        }
    }
}
