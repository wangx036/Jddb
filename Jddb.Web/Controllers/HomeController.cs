using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using Jddb.Web.Quartz;
using mtTools;
using Microsoft.AspNetCore.Mvc;
using Quartz.Util;
using SqlSugar;

namespace Jddb.Web.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    [Route("")]
    public class HomeController : BaseController
    {
        private WebServer _webService;
        private AuctionService _auctionService;
        private PriceCacheServer _priceCacheServer;

        public HomeController(WebServer webService,AuctionService auctionService,PriceCacheServer priceCacheServer)
        {
            _webService = webService;
            _auctionService = auctionService;
            _priceCacheServer = priceCacheServer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Content("成功");
        }

        /// <summary>
        /// 获取在拍商品列表
        /// </summary>
        /// <param name="pageIndex">页码，默认1</param>
        /// <param name="pageSize">每页条数，默认100</param>
        /// <param name="status">状态，""：全部，"1"：即将夺宝，"2"：正在夺宝，默认2</param>
        /// <param name="groupId">类别</param>
        /// <param name="keyword">关键词，以空格分隔</param>
        /// <returns></returns>
        [HttpGet("getList")]
        public async Task<ApiResult<List<AuctionInfo>>> GetList(int pageIndex = 1, int pageSize = 100, string status = "2", string groupId = "",string keyword="")
        {
            var list = _webService.GetAuctionList(pageIndex, pageSize, status, groupId);

            if (!keyword.IsNullOrWhiteSpace())
            {
                keyword = keyword.Replace(" ", "%");
                list = list.Where(o => o.ProductName.Contains(keyword)).ToList();
            }

            // 参考价格
            var usednos = list.Select(o => o.UsedNo).Distinct().ToList();
            var tips = await _priceCacheServer.GetListAsync(o=>usednos.Contains(o.UsedNo));
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

        /// <summary>
        /// 获取历史商品列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="keyword">关键词，以空格分隔</param>
        /// <returns></returns>
        [HttpGet("getUsedList")]
        public async Task<ApiResult<Page<PriceCache>>> GetUsedAuctionList(int pageIndex = 1, int pageSize = 50, string keyword = "")
        {
            keyword = keyword.Replace(" ", "%");
            var page = await _priceCacheServer.GetPagesAsync(pageIndex, pageSize,o=>o.ProductName.Contains(keyword),o=>o.Count,OrderByEnmu.Desc);
            return await SuccessTask(data:page);
        }


        [HttpPost("testEmail")]
        public async Task TestEmail()
        {
            var jobOffer=new JobOffer();
            var body = $@"
<br>商品编号：<a href='https://paipai.jd.com/auction-detail/{jobOffer.AuctionId}'>{jobOffer.AuctionId}</a>
<br>商品名称：<a href='https://paipai.jd.com/auction-detail/{jobOffer.AuctionId}'>{jobOffer.AuctionName}</a>
<br>中拍价格：{jobOffer.OfferPrice}
<br>中拍时间：{jobOffer.OfferTime}";
            MailHelper.SendEmail("wx036@qq.com", "京东夺宝中拍提醒", body);
        }

    }
}
