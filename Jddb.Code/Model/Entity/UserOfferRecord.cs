using SqlSugar;

namespace Jddb.Code.Model
{
    /// <summary>
    /// 出价记录
    /// </summary>
    [SugarTable("user_offerrecord")]
    public class UserOfferRecord
    {
        /// <summary>
        /// Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 商品id
        /// </summary>
        public int AuctionId { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string UsedNo { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 出价
        /// </summary>
        public double OfferPrice { get; set; }

        /// <summary>
        /// 成交价
        /// </summary>
        public double FinalPrice { get; set; }

        /// <summary>
        /// 出价类型
        /// </summary>
        public int OfferType { get; set; }

    }
}