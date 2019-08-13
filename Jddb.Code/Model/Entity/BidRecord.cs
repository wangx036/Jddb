using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace Jddb.Code.Model
{
    public class BidRecord
    {
        public uint Id { get; set; }
        /// <summary>
        /// 商品id
        /// </summary>
        public uint AuctionId { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string UsedNo { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public double OfferPrice { get; set; }
        /// <summary>
        /// 出价时间
        /// </summary>
        [SugarColumn(ColumnName= "OfferTime")]
        public long Created { get; set; }
        /// <summary>
        /// 出价人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 参加人昵称
        /// </summary>
        public string UserNickName { get; set; }
        /// <summary>
        /// 是否是成交价
        /// </summary>
        public bool IsFinalPrice { get; set; } = false;
    }
}
