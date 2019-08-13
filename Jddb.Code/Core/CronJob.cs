using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jddb.Code.Common;
using Jddb.Code.Core.Db;
using Jddb.Code.Model;

namespace Jddb.Code.Core
{
    public class CronJob
    {
        private Business _business=new Business();
        private DbContext _dbContext=new DbContext();
        private int _spanMins = 2;


        public void InsertAuctionAndBidRecord()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        // 1.每天8点-24点执行
                        if (DateTime.Now.Hour < 8)
                        {
                            var timespan8 = Convert.ToDateTime($"{DateTime.Today:yyyy-MM-dd} 8:00:00").ToTimeStamp() -
                                            DateTime.Now.ToTimeStamp();
                            Thread.Sleep((int) timespan8);
                            continue;
                        }

                        // 2.插入爬取日志
                        var crawLog = InsertAuctionInfo();
                        crawLog.BidRecordCount = InsertBidRecord();
                        _dbContext.Db.Insertable<CrawlLog>(crawLog).ExecuteCommand();


                    }
                    catch (Exception e)
                    {
                        //var log=new Logger("info");
                        //log.Info(msg:e.Message);
                    }
                    finally
                    {
                        // 4.sleep 5min
                        Thread.Sleep(_spanMins * 60 * 1000);
                    }

                }

            });
        }



        /// <summary>
        ///  插入商品
        /// </summary>
        public CrawlLog InsertAuctionInfo()
        {
            var model=new CrawlLog();
            // 2.获取列表 
            var auctionList = _business.GetAuctionList(pageSize:100,status:"2");
            if(auctionList==null)
                return model;

            var existIds = _dbContext.Db.Queryable<AuctionInfo>()
                .Where(o => o.EndTime >= DateTime.Today.ToTimeStampMs()).Select(o => o.Id).ToList();
            var newList = auctionList.Where(o => !existIds.Contains(o.Id)).ToList();

            // 3.插入列表
            var rev = _dbContext.Db.Insertable<AuctionInfo>(newList).ExecuteCommand();

            model.AuctionCount = rev;
            model.DelAuctionCount = auctionList.Count-rev;
            return model;
        }


        /// <summary>
        /// 插入出价记录
        /// </summary>
        public int InsertBidRecord()
        {
            var recordCount = 0;

            // 2.获取已经结束且没有插入出价详情的商品 
            var auctions = _dbContext.Db.Queryable<AuctionInfo>()
                .Where(o => o.EndTime <  DateTime.Now.AddMinutes(-_spanMins).ToTimeStampMs())
                .Where(o => !o.HasBidRecord)
                .Select<AuctionInfo>(o => new AuctionInfo() {Id = o.Id, RecordCount = o.RecordCount})
                .ToList();

            // 3.获取出价记录
            var brList = new List<BidRecord>();
            for (var i = 0; i < auctions.Count; i++)
            {
                var item = auctions[i];
                var br = _business.GetBidRecords(item.Id);
                if (br == null)
                    continue;

                // 标记最高价,未流拍
                if(br.Any())
                    br.OrderByDescending(o => o.OfferPrice).First().IsFinalPrice = true;
                item.RecordCount = br.Count;
                item.HasBidRecord = true;

                brList.AddRange(br);
                item.CurrentPrice = br.Any() ? br.OrderByDescending(o => o.OfferPrice).First().OfferPrice : 1;

                if ((i+1) % 20 == 0 || i == auctions.Count)
                {
                    // 3.插入出价记录，并更新auction.hasbidrecord
                    recordCount += _dbContext.Db.Insertable<BidRecord>(brList).ExecuteCommand();
                    _dbContext.Db.Updateable<AuctionInfo>(auctions).UpdateColumns(o => new { o.RecordCount, o.HasBidRecord ,o.CurrentPrice})
                        .ExecuteCommand();

                    brList.Clear();
                }
            }

            return recordCount;

        }

    }
}
