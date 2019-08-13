using System;
using Jddb.Core.Extend;
using SqlSugar;

namespace Jddb.Core.Model
{
    /// <summary>
    /// 出价记录
    /// </summary>
    [SugarTable("user_offerrecord")]
    public class UserOfferRecord:Entity
    {

        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string UsedNo { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string PrimaryPic { get; set; }

        /// <summary>
        /// 出价
        /// </summary>
        public double OfferPrice { get; set; }

        /// <summary>
        /// 成交价
        /// </summary>
        public int SpanPrice { get; set; }

        /// <summary>
        /// 出价类型
        /// </summary>
        public Enums.OfferTypeEnum OfferType { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public Enums.JobTypeEnum JobType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }=DateTime.Now;

        /// <summary>
        /// 竞拍数量
        /// </summary>
        public int AuctionCount { get; set; } = 0;
    }
}