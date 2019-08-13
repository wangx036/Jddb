using Newtonsoft.Json;
using SqlSugar;

namespace Jddb.Code.Model
{
    /// <summary>
    /// 商品信息
    /// </summary>
    public class AuctionInfo
    {
        /// <summary>
        /// 商品id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public uint Id { get; set; }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string UsedNo { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图
        /// </summary>
        public string PrimaryPic { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public long EndTime { get; set; }
        /// <summary>
        /// 价格上限
        /// </summary>
        public double CappedPrice { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        public double CurrentPrice { get; set; }
        /// <summary>
        /// 出价次数
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 成色
        /// </summary>
        public string Quality { get; set; }
        /// <summary>
        /// 分类1
        /// </summary>
        public int Category1 { get; set; }
        /// <summary>
        /// 分类1名称
        /// </summary>
        public string Category1Name { get; set; }
        /// <summary>
        /// 分类2
        /// </summary>
        public int Category2 { get; set; }
        /// <summary>
        /// 分类2名称
        /// </summary>
        public string Category2Name { get; set; }
        /// <summary>
        /// 分类3
        /// </summary>
        public int Category3 { get; set; }
        /// <summary>
        /// 分类3名称
        /// </summary>
        public string Category3Name { get; set; }

        /// <summary>
        /// 是否有出价记录
        /// </summary>
        [JsonIgnore]
        public bool HasBidRecord { get; set; }

        ///// <summary>
        ///// 价格提示
        ///// </summary>
        //[SugarColumn(IsIgnore = true)]
        //public PriceTip PriceTip { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public double MinPrice { get; set; }
        /// <summary>
        /// 平均价
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public double AvgPrice { get; set; }
        /// <summary>
        /// 成交数
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int Count { get; set; }
    }
}