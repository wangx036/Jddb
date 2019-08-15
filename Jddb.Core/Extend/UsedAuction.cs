using Jddb.Core.Model;

namespace Jddb.Core.Extend
{
    /// <summary>
    /// 商品列表（剔除重复）
    /// </summary>
    public class UsedAuction
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public string UsedNo { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 成色
        /// </summary>
        public string Quality { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string PrimaryPic { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public double MinPrice { get; set; }
        /// <summary>
        /// 平均价
        /// </summary>
        public double AvgPrice { get; set; }
        /// <summary>
        /// 成交数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// quartz信息（竞拍类型、价格等）
        /// </summary>
        public JobOffer OfferInfo { get; set; }
    }
}