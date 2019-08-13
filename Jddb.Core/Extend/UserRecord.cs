using System;
using System.Collections.Generic;
using System.Text;

namespace Jddb.Core.Extend
{
    /// <summary>
    /// jd 获取京东我的夺宝列表
    /// </summary>
    public class UserRecord
    {
        /// <summary>
        /// 商品id
        /// </summary>
        public int AuctionId { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string UsedNo { get; set; }
        /// <summary>
        /// 出价状态
        /// </summary>
        public BidTypeEnum Status { get; set; }
    }

    /// <summary>
    /// 记录状态
    /// </summary>
    public enum BidTypeEnum
    {
        /// <summary>
        /// 未结束
        /// </summary>
        NotFinished=1,
        /// <summary>
        /// 中拍未付款
        /// </summary>
        NotPay=2,
        /// <summary>
        /// 中拍已付款
        /// </summary>
        Get=3,
        /// <summary>
        /// 未中拍
        /// </summary>
        Fail=4
    }
}
