namespace Jddb.Core.Extend
{
    /// <summary>
    /// 常量
    /// </summary>
    public static class MyKeys
    {
        /// <summary>
        /// redis key 以用户为组(JobUser_1)
        /// </summary>
        public static string RedisJobUser(int? userid=null)
        {
            if (userid == null)
                return "JobUser_";
            return "JobUser_" + userid;
        }
        /// <summary>
        /// redis key 用户信息(UserInfo_1)
        /// </summary>
        public static string RedisUserInfo(int userid)
        {
            return "UserInfo_" + userid;
        }

        /// <summary>
        /// 统计的商品价格信息
        /// </summary>
        public static string RedisPriceTip = "PriceTip";

        /// <summary>
        /// quartz 组(user1)
        /// </summary>
        public static string QuartzGroup(int userid)
        {
            return "user" + userid;
        }

        /// <summary>
        /// quartz 商品id(auction123456)
        /// </summary>
        public static string QuartzAuctionId(int? auctionId=null)
        {
            if (auctionId == null)
                return "auctionid";
            return "auctionid" + auctionId;
        }

        /// <summary>
        /// quartz 商品编码（used123456/used123456_auctionid123456）
        /// </summary>
        /// <param name="usedNo">商品编码</param>
        /// <param name="auctionId">商品id</param>
        /// <returns></returns>
        public static string QuartzUsedNo(string usedNo,int? auctionId=null)
        {
            if (auctionId == null)
                return "used" + usedNo;
            return "used" + usedNo + "_auctionid" + auctionId;
        }


    }
}