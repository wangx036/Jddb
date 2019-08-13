using System;
using Jddb.Core.Extend;
using Newtonsoft.Json;
using SqlSugar;

namespace Jddb.Core.Model
{
    /// <summary>
    /// 出价任务记录表
    /// </summary>
    [SugarTable("job_offer")]
    public class JobOffer:Entity
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// job组
        /// </summary>
        public string JobGroup { get; set; }

        /// <summary>
        /// job名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// *商品编码
        /// </summary>
        public string UsedNo { get; set; }

        /// <summary>
        /// *商品id
        /// </summary>
        public int AuctionId { get; set; }
        /// <summary>
        /// *商品名称
        /// </summary>
        public string AuctionName { get; set; }

        /// <summary>
        /// *最高价
        /// </summary>
        public int MaxPrice { get; set; }

        /// <summary>
        /// 加价幅度
        /// </summary>
        public int SpanPrice { get; set; } = 5;

        /// <summary>
        /// 出价
        /// </summary>
        public int OfferPrice { get; set; }

        /// <summary>
        /// *出价类型（1.一口价，2.加价）
        /// </summary>
        public Enums.OfferTypeEnum OfferType { get; set; } =  Enums.OfferTypeEnum.OnePrice;
        /// <summary>
        /// *任务类型（1.商品id，2.商品编码单次，3.商品编码持续）
        /// </summary>
        public Enums.JobTypeEnum JobType { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 出价时间
        /// </summary>
        public DateTime OfferTime { get; set; }=DateTime.Now;
        /// <summary>
        /// 商品过期时间 时间戳（JobType=1时必传）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [JsonIgnore]
        public long EndTime { get; set; }

    }

   
}