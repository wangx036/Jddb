using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using Quartz;

namespace Jddb.Web.Quartz
{
    /// <summary>
    /// 爬取数据（并添加竞价任务）任务
    /// </summary>
    public class CrawlJob:IJob
    {
        private int _spanMins = 2;
        private int _pageSize = 100;

        private WebServer _webServer=new WebServer();
        private AuctionService _auctionService=new AuctionService();
        private SchedulerCenter _schedulerCenter =new SchedulerCenter();


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var crawlTime = DateTime.Now;
                var tuple = InsertAuctionInfo();
                if(tuple==null) return;
                AddUsedNoJob(tuple.Item2);
                var bidRecordCount = InsertBidRecord();

                var crawLog=new CrawlLog()
                {
                    AuctionCount = tuple.Item1.Count,
                    DelAuctionCount = tuple.Item2.Count- tuple.Item1.Count,
                    BidRecordCount = bidRecordCount,
                    CrawlTime = crawlTime
                };
                _auctionService.Db.Insertable<CrawlLog>(crawLog).ExecuteCommand();
            }
            catch (Exception e)
            {
                Logger.Default.Error("[CrawlJob.Execute]"+e.Message, e);
            }
        }


        /// <summary>
        ///  插入商品
        /// </summary>
        public Tuple<List<AuctionInfo>, List<AuctionInfo>> InsertAuctionInfo()
        {
            // 2.获取列表 
            var auctionList = _webServer.GetAuctionList(pageSize: _pageSize, status: "2");
            if (auctionList == null)
                return null;

            var existIds = _auctionService.Db.Queryable<AuctionInfo>()
                .Where(o => o.EndTime >= DateTime.Today.ToTimeStampMs()).Select(o => o.Id).ToList();
            var newList = auctionList.Where(o => !existIds.Contains(o.Id)).ToList();

            // 3.插入列表
            var rev = _auctionService.Db.Insertable<AuctionInfo>(newList).ExecuteCommand();

            return Tuple.Create(newList, auctionList);
        }


        /// <summary>
        /// 插入出价记录
        /// </summary>
        public int InsertBidRecord()
        {
            var recordCount = 0;

            // 2.获取已经结束且没有插入出价详情的商品 
            var auctions = _auctionService.Db.Queryable<AuctionInfo>()
                .Where(o => o.EndTime < DateTime.Now.AddMinutes(-_spanMins).ToTimeStampMs())
                .Where(o => !o.HasBidRecord)
                .Select<AuctionInfo>(o => new AuctionInfo() { Id = o.Id, RecordCount = o.RecordCount })
                .ToList();

            // 3.获取出价记录
            var brList = new List<BidRecord>();
            for (var i = 0; i < auctions.Count; i++)
            {
                var item = auctions[i];
                var br = _webServer.GetBidRecords(item.Id);
                if (br == null)
                    continue;

                // 标记最高价,未流拍
                if (br.Any())
                    br.OrderByDescending(o => o.OfferPrice).First().IsFinalPrice = true;
                item.RecordCount = br.Count;
                item.HasBidRecord = true;

                brList.AddRange(br);
                item.CurrentPrice = br.Any() ? br.OrderByDescending(o => o.OfferPrice).First().OfferPrice : 1;

                if ((i + 1) % 20 == 0 || i == auctions.Count)
                {
                    // 3.插入出价记录，并更新auction.hasbidrecord
                    recordCount += _auctionService.Db.Insertable<BidRecord>(brList).ExecuteCommand();
                    _auctionService.Db.Updateable<AuctionInfo>(auctions).UpdateColumns(o => new { o.RecordCount, o.HasBidRecord, o.CurrentPrice })
                        .ExecuteCommand();

                    brList.Clear();
                }
            }

            return recordCount;

        }

        /// <summary>
        /// 获取列表中，如果有需要竞拍的商品，则添加到任务
        /// </summary>
        /// <param name="auctionList"></param>
        /// <returns></returns>
        public async Task AddUsedNoJob(List<AuctionInfo> auctionList)
        {
            var keys = RedisHelper.Keys($"{MyKeys.RedisJobUser()}*");
            foreach (var key in keys)
            {
                var joblist = RedisHelper.Get<List<JobOffer>>(key);
                foreach (var auction in auctionList.Where(a => joblist.Select(job => job.UsedNo).Contains(a.UsedNo)))
                {
                    var job = joblist.Find(j => j.UsedNo == auction.UsedNo);
                    job.AuctionId = auction.Id;
                    job.OfferTime = auction.EndTime.ToDateTime().AddSeconds(-1);
                    job.JobName = MyKeys.QuartzUsedNo(auction.UsedNo,auction.Id);
                    job.AuctionName = auction.ProductName;

                    await _schedulerCenter.AddScheduleJobAsync(job);
                }
            }
        }

    }
}