using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jddb.Service
{
    public class WebServer
    {


        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="status"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<AuctionInfo> GetAuctionList(int pageNo = 1, int pageSize = 100, string status = "", string groupId = "")
        {
            var url = "https://used-api.jd.com/auction/list?";
            url +=
                $"pageNo={pageNo}&pageSize={pageSize}&category1=&status={status}&orderDirection=1&orderType=1&groupId={groupId}";

            var res = HttpHelper.GetResponse<dynamic>(url);
            string resStr = Convert.ToString(res.data.auctionInfos);
            var list = JsonConvert.DeserializeObject<List<AuctionInfo>>(resStr);

            // 京东服务器时间
            var jdTime = (long)res.data.systemTime;
            // 本地服务器时间
            var thisTime = DateTime.Now.ToTimeStampMs();
            foreach (var item in list)
            {
                item.EndTime = item.EndTime - jdTime + thisTime;
            }

            return list;
        }

        /// <summary>
        /// 出价
        /// </summary>
        /// <param name="auctionId">商品id</param>
        /// <param name="price">价格</param>
        /// <param name="thor">登录cookie</param>
        /// <returns></returns>
        public ApiResult Offer(int auctionId, int price, string thor)
        {
            var referer = "https://paipai.jd.com/auction-detail/" + auctionId;
            var offerUrl = "https://used-api.jd.com/auctionRecord/offerPrice";
            var header = new Dictionary<string, string>()
            {
                {"referer",referer },
                { "cookie","thor="+thor }
            };
            var param = $"auctionId={auctionId}&price={price}&token={DateTime.Now.ToShortTimeString()}";
            var res = HttpHelper.PostResponseFormData<dynamic>(offerUrl, param, header);
            string msg = res.message.ToString();
            int code = int.Parse(res.code.ToString());
            return new ApiResult()
            {
                IsSuccess = code==200,
                StatusCode = code,
                Message = msg
            };
        }

        /// <summary>
        /// 获取最新商品价格
        /// </summary>
        /// <param name="auctionIds">商品ids</param>
        /// <returns></returns>
        public List<CurrentInfo> GetCurrentPrices(List<int> auctionIds)
        {
            auctionIds = auctionIds.Distinct().ToList();
            var url =
                $"https://used-api.jd.com/auctionRecord/batchCurrentInfo?auctionId={string.Join(",", auctionIds)}";
            var res = HttpHelper.GetResponse<dynamic>(url);

            var list=new List<CurrentInfo>();
            foreach (var id in auctionIds)
            {
                list.Add(new CurrentInfo()
                {
                    AuctionId = id,
                    CurrentPrice = res.data[$"{id}"].currentPrice
                });
            }

            return list;
        }

        /// <summary>
        /// 获取最新商品价格
        /// </summary>
        /// <param name="auctionId">商品id</param>
        /// <returns></returns>
        public CurrentInfo GetCurrentPrice(int auctionId)
        {
            return GetCurrentPrices(new List<int>() {auctionId}).First();
        }

        /// <summary>
        /// 获取出价详细
        /// </summary>
        /// <param name="auctionId">商品id</param>
        /// <returns></returns>
        public List<BidRecord> GetBidRecords(int auctionId)
        {
            var url = $"https://used-api.jd.com/auction/bidrecords?auctionId={auctionId}";
            var res = HttpHelper.GetResponse<dynamic>(url);
            string data = res.data.ToString();
            var list = JsonConvert.DeserializeObject<List<BidRecord>>(data);

            return list;
        }

        /// <summary>
        /// 获取用户名称
        /// </summary>
        /// <param name="thor"></param>
        /// <returns></returns>
        public UserThor GetUserInfo(string thor)
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
            var model = JsonConvert.DeserializeObject<UserThor>(resStr);
            model.ThorEncrypt = thor.DESEncrypt();
            return model;
        }

        /// <summary>
        /// 获取用户的出价列表
        /// </summary>
        /// <param name="thor"></param>
        /// <returns></returns>
        public List<UserRecord> GetUserRecords(string thor)
        {
            var url = "https://used-api.jd.com/auctionRecord/queryRecordList?showStatus=0&pageSize=20&pageNum=1";
            var header = new Dictionary<string, string>()
            {
                {"referer","https://paipai.jd.com/" },
                { "cookie","thor="+thor }
            };
            var res = HttpHelper.GetResponse<dynamic>(url);
            string data = res.data.bidRecordList.ToString();
            var list = JsonConvert.DeserializeObject<List<UserRecord>>(data);
            return list;
        }


    }
}
