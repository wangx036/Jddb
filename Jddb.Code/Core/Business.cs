using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jddb.Code.Common;
using Jddb.Code.Core.Db;
using Jddb.Code.Model;
using Newtonsoft.Json;

namespace Jddb.Code.Core
{
    public class Business
    {
        private ComplexQuery _query =new ComplexQuery();


        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="status"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<AuctionInfo> GetAuctionList(int pageNo=1, int pageSize=100, string status="", string groupId="",bool isTip=false)
        {
            try
            {
                var url = "https://used-api.jd.com/auction/list?";
                url +=
                    $"pageNo={pageNo}&pageSize={pageSize}&category1=&status={status}&orderDirection=1&orderType=1&groupId={groupId}";

                var res = HttpHelper.GetResponse<dynamic>(url);
                string resStr = Convert.ToString(res.data.auctionInfos);
                var list = JsonConvert.DeserializeObject<List<AuctionInfo>>(resStr);

                if (isTip)
                {
                    var usednos = list.Select(o => o.UsedNo).Distinct().ToList();
                    var tips = _query.PriceTips(usednos);

                    // 京东服务器时间
                    var jdTime = (long)res.data.systemTime;
                    // 本地服务器时间
                    var thisTime = DateTime.Now.ToTimeStampMs();
                    foreach (var item in list)
                    {
                        var priceTip = tips.FirstOrDefault(o => o.UsedNo == item.UsedNo);
                        if (priceTip != null)
                        {
                            item.MinPrice = priceTip.MinPrice;
                            item.AvgPrice = priceTip.AvgPrice;
                            item.Count = priceTip.Count;
                        }
                        item.EndTime = item.EndTime - jdTime + thisTime;
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// 出价
        /// </summary>
        /// <param name="auctionId">商品id</param>
        /// <param name="price">价格</param>
        /// <param name="thor">登录cookie</param>
        /// <returns></returns>
        public string Offer(string auctionId, int price,string thor)
        {
            var referer = "https://paipai.jd.com/auction-detail/" + auctionId;
            var offerUrl = "https://used-api.jd.com/auctionRecord/offerPrice";
            var header = new Dictionary<string, string>()
            {
                {"referer",referer },
                { "cookie","thor="+thor }
            };
            var param = $"auctionId={auctionId}&price={price}";
            var postData = new { auctionId = auctionId, price = price }.ToString();
            var res = HttpHelper.PostResponseFormData<dynamic>(offerUrl, param, header);
            return res.message;
        }

        /// <summary>
        /// 获取出价详细
        /// </summary>
        /// <param name="auctionId">商品id</param>
        /// <returns></returns>
        public List<BidRecord> GetBidRecords(uint auctionId)
        {
            try
            {
                var url = $"https://used-api.jd.com/auction/bidrecords?auctionId={auctionId}";
                var res = HttpHelper.GetResponse<dynamic>(url);
                string data = res.data.ToString();
                var list = JsonConvert.DeserializeObject<List<BidRecord>>(data);

                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取用户名称
        /// </summary>
        /// <param name="thor"></param>
        /// <returns></returns>
        public UserInfo GetUserInfo(string thor)
        {
            var url = "https://passport.jd.com/user/petName/getUserInfoForMiniJd.action";
            var header = new Dictionary<string, string>()
            {
                {"referer","https://paipai.jd.com/auction-log" },
                { "cookie","thor="+thor }
            };
            var resStr = HttpHelper.GetResponse<string>(url, header);
            resStr = resStr.Replace(Convert.ToChar(10).ToString(), "").Replace(Convert.ToChar(13).ToString(), "")
                .TrimStart('(').TrimEnd(')');
            var model=JsonConvert.DeserializeObject<UserInfo>(resStr);
            model.Thor = thor;
            return model;
        }

        public object GetUserRecords(string thor)
        {
            var url = "https://used-api.jd.com/auctionRecord/queryRecordList?showStatus=2&pageSize=30&pageNum=1";
            var header = new Dictionary<string, string>()
            {
                {"referer","https://paipai.jd.com/" },
                { "cookie","thor="+thor }
            };
            var res = HttpHelper.GetResponse<dynamic>(url);
            string data = res.data.bidRecordList.ToString();
            var list = JsonConvert.DeserializeObject<List<BidRecord>>(data);
            return list;
        }
        

    }
}
