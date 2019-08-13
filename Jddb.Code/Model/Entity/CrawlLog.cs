using System;
using SqlSugar;

namespace Jddb.Code.Model
{
    /// <summary>
    /// 爬取日志
    /// </summary>
    public class CrawlLog
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        /// <summary>
        /// 导入时间
        /// </summary>
        public DateTime CrawlTime { get; set; }=DateTime.Now;
        /// <summary>
        /// 插入的商品条数
        /// </summary>
        public int AuctionCount { get; set; }
        /// <summary>
        /// 剔除的商品条数
        /// </summary>
        public int DelAuctionCount { get; set; }
        /// <summary>
        /// 出价条数
        /// </summary>
        public int BidRecordCount { get; set; }

    }
}