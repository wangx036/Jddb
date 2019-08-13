using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using mtTools;
using Microsoft.AspNetCore.Mvc;

namespace Jddb.Web.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private WebServer _webService;
        private AuctionService _auctionService;

        public HomeController(WebServer webService,AuctionService auctionService)
        {
            _webService = webService;
            _auctionService = auctionService;
        }

        [HttpGet("index")]
        public IActionResult Index()
        {
            return Content("成功");
        }

        /// <summary>
        /// 获取在拍列表
        /// </summary>
        /// <param name="pageIndex">页码，默认1</param>
        /// <param name="pageSize">每页条数，默认100</param>
        /// <param name="status">状态，""：全部，"1"：即将夺宝，"2"：正在夺宝，默认2</param>
        /// <param name="groupId">类别</param>
        /// <returns></returns>
        [HttpGet("getlist")]
        public async Task<ApiResult<List<AuctionInfo>>> GetList(int pageIndex = 1, int pageSize = 100, string status = "2", string groupId = "")
        {
            throw new Exception("测试错误");
            var list = _webService.GetAuctionList(pageIndex, pageSize, status, groupId);

            // 参考价格
            var usednos = list.Select(o => o.UsedNo).Distinct().ToList();
            var tips = await _auctionService.PriceTipsAsync(usednos);
            foreach (var item in list)
            {
                var priceTip = tips.FirstOrDefault(o => o.UsedNo == item.UsedNo);
                if (priceTip != null)
                {
                    item.MinPrice = priceTip.MinPrice;
                    item.AvgPrice = priceTip.AvgPrice;
                    item.Count = priceTip.Count;
                }
            }

             return await SuccessTask(data:list);

        }

        [HttpGet("GetUsedAuctionList")]
        public async Task<ApiResult<Page<UsedAuction>>> GetUsedAuctionList(int pageIndex = 1, int pageSize = 50, string keyword = "")
        {
            var page = await _auctionService.UsedAuctions(keyword, pageIndex, pageSize);
            return await SuccessTask(data:page);
        }


        [HttpPost("offer")]
        public ActionResult Offer(string id, int price)
        {
            var referer = "https://paipai.jd.com/auction-detail/" + id;
            var offerUrl = "https://used-api.jd.com/auctionRecord/offerPrice";
            var header = new Dictionary<string, string>()
            {
                {"referer",referer },             {"cookie","thor=A91B391391BB85441F90B387B4BF42CEFADDDD7ADD90CDEEA466CAE14036574E90C305DCA01675579CD9C71104F651C5A0DE65E037A868B86044DDE8ABF56C05CA4873FF7647C3227EB1F244A6F7A454EC28683A7808A05AB9710DA615398BB52A46A926154F1245B4C2145D2E37A2737045EC1E9327D2B7A82C461C8D7DD14FBC2336E10A6FEDF0A843B6759810994B" }
            };
            var postData = new {auctionId = id, price = price}.ToString();
            var res = HttpHelper.PostResponseFormData<dynamic>(offerUrl, $"auctionId={id}&price={price}", header);
            return Ok(res.ToString());
        }

        /// <summary>
        /// 获取最新价格
        /// </summary>
        /// <param name="auctionIds"></param>
        [HttpPost("getcurrentprice")]
        public List<CurrentInfo> GetCurrentPrice(List<int> auctionIds)
        {
            return _webService.GetCurrentPrices(auctionIds);
        }

        /// <summary>
        /// 发生邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost("sendMail")]
        public async Task SendMail()
        {
            MailHelper.SendEmail("wx036@qq.com", "测试邮件", "<a href=\"www.linanwx.com\"> www.linanwx.com</a>");
        }


    }
}
