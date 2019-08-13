using System;
using System.Collections.Generic;
using System.Text;

namespace Jddb.Code.Model
{
    /// <summary>
    /// 价格参考，min,avg,count
    /// </summary>
    public class PriceTip
    {
        /// <summary>
        /// 商品编码
        /// </summary>
        public string UsedNo { get; set; }
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
    }
}
