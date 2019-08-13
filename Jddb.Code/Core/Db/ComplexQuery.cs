using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Jddb.Code.Common;
using Jddb.Code.Model;
using SqlSugar;

namespace Jddb.Code.Core.Db
{
    public class ComplexQuery
    {
        private DbContext _dbContext =new DbContext();
        private BaseServer<AuctionInfo> _auctionServer =new BaseServer<AuctionInfo>();

        //        public List<PriceTip> PriceTips(List<string> usednos)
        //        {
        //            var strNos = $"'{string.Join("','", usednos)}'";

        //            var sql=$@"select a.usedno,a.medianprice,b.minprice,b.avgprice from 
        //(SELECT usedno,round(currentprice) medianprice,
        //( CASE usedno WHEN @curCode THEN @curRow := @curRow + 1 ELSE @curRow := 1 AND @curCode := usedno END ) AS sort
        //FROM auctioninfo t, ( SELECT @curRow := 0, @curCode := '' ) rt where hasbidrecord=1 and usedno in ({strNos})
        //ORDER BY usedno, currentprice) a
        //inner join 
        //(SELECT USEDNO,CEIL(COUNT(*)/2) median,round(min(currentprice)) minprice,round(avg(currentprice)) avgprice from auctioninfo where hasbidrecord=1 and usedno in ({strNos}) group by usedno) b
        //on a.usedno=b.usedno and a.sort=b.median";

        //            var list = _dbContext.Db.Queryable<PriceTip>(sql).ToList();
        //            return list;
        //        }

        /// <summary>
        /// 获取价格参考（近一个月）
        /// </summary>
        /// <param name="usednos"></param>
        /// <returns></returns>
        public List<PriceTip> PriceTips(List<string> usednos)
        {
           var list =  _dbContext.Db.Queryable<AuctionInfo>().GroupBy(a => a.UsedNo)
                .Where(a => a.HasBidRecord && usednos.Contains(a.UsedNo) && a.EndTime>=DateTime.Now.AddMonths(-1).ToTimeStampMs())
                .Select<PriceTip>(a => new PriceTip()
                {
                    UsedNo = a.UsedNo,
                    MinPrice = SqlFunc.AggregateMin(a.CurrentPrice),
                    AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice),0),
                    Count = SqlFunc.AggregateCount(a.Id)
                }).ToList();

            return list;
        }

        /// <summary>
        /// 获取商品列表（剔除同样商品）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Page<UsedAuction> UsedAuctions(string keyword,int pageIndex,int pageSize)
        {
            var page = _dbContext.Db.Queryable<AuctionInfo>().GroupBy(a => new {a.UsedNo, a.ProductName, a.Quality})
                .Where(a => a.HasBidRecord)
                .Where(a => a.ProductName.Contains(keyword))
                .Select(a => new UsedAuction()
                {
                    UsedNo = a.UsedNo,
                    ProductName = a.ProductName,
                    PrimaryPic = SqlFunc.AggregateMax(a.PrimaryPic),
                    Quality = a.Quality,
                    Count = SqlFunc.AggregateCount(a.Id),
                    MinPrice = SqlFunc.AggregateMin(a.CurrentPrice),
                    AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice),0)
                }).ToPage(pageIndex, pageSize);

            return page;
        }

    }
}