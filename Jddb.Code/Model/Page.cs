using System.Collections.Generic;

namespace Jddb.Code.Model
{
    public class Page<T>
    {
        /// <summary>
        /// 当前页索引
        /// </summary>
        public long CurrentPage { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalPages  => TotalItems != 0 ? (TotalItems % PageSize) == 0 ? (TotalItems / PageSize) : (TotalItems / PageSize) + 1 : 0;
        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalItems { get; set; }
        /// <summary>
        /// 每页的记录数
        /// </summary>
        public long PageSize { get; set; }
        /// <summary>
        /// 数据集
        /// </summary>
        public List<T> Items { get; set; }
    }

    public class PageParm
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int page { get; set; } = 1;

        /// <summary>
        /// 每页总条数
        /// </summary>
        public int limit { get; set; } = 20;

    }
}
