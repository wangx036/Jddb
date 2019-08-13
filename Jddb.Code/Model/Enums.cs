namespace Jddb.Code.Model
{
    public class Enums
    {
        /// <summary>
        /// 排序
        /// </summary>
        public enum OrderEnum
        {
            /// <summary>
            /// 排序Asc
            /// </summary>
            Asc = 1,

            /// <summary>
            /// 排序Desc
            /// </summary>
            Desc = 2
        }

        /// <summary>
        /// 出价类型
        /// </summary>
        public enum OfferTypeEnum
        {
            /// <summary>
            /// 商品id单次
            /// </summary>
            SingleAuction=1,
            /// <summary>
            /// 商品编码单次
            /// </summary>
            SingleUsed=2,
            /// <summary>
            /// 商品编码持续
            /// </summary>
            MultipleUsed=3
        }
    }
}