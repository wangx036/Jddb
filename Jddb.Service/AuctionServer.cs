using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using SqlSugar;

namespace Jddb.Service
{
    public class AuctionService:BaseServer<AuctionInfo>
    {
        /// <summary>
        /// 获取价格参考（近一个月）
        /// </summary>
        /// <param name="usednos"></param>
        /// <returns></returns>
        public async Task<List<PriceTip>> PriceTipsAsync(List<string> usednos=null, bool Async = true)
        {
            var quary = Db.Queryable<AuctionInfo>().GroupBy(a => a.UsedNo)
                .Where(a => a.HasBidRecord && a.EndTime >= DateTime.Now.AddMonths(-1).ToTimeStampMs())
                .WhereIF(usednos != null && usednos.Any(), a => usednos.Contains(a.UsedNo))
                .Select<PriceTip>(a => new PriceTip()
                {
                    UsedNo = a.UsedNo,
                    MinPrice = SqlFunc.AggregateMin(a.CurrentPrice),
                    AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice), 0),
                    Count = SqlFunc.AggregateCount(a.Id)
                });
            var list = Async ? await quary.ToListAsync() : quary.ToList();

            return list;
        }

        /// <summary>
        /// 获取商品列表（剔除同样商品）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<Page<UsedAuction>> UsedAuctions(string keyword, int pageIndex, int pageSize, bool Async = true)
        {
            var quary = Db.Queryable<AuctionInfo>().GroupBy(a => new { a.UsedNo, a.ProductName, a.Quality })
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
                    AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice), 0)
                });
            var page = Async ? await quary.ToPageAsync(pageIndex, pageSize) : quary.ToPage(pageIndex, pageSize);

            return page;
        }

        /// <summary>
        /// 获取商品列表（剔除同样商品）
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<List<UsedAuction>> UsedAuctions(Expression<Func<AuctionInfo, bool>> where, bool Async = true)
        {
            var quary = Db.Queryable<AuctionInfo>().GroupBy(a => new { a.UsedNo, a.ProductName, a.Quality })
                .Where(a => a.HasBidRecord)
                .Where(where)
                .Select(a => new UsedAuction()
                {
                    UsedNo = a.UsedNo,
                    ProductName = a.ProductName,
                    PrimaryPic = SqlFunc.AggregateMax(a.PrimaryPic),
                    Quality = a.Quality,
                    Count = SqlFunc.AggregateCount(a.Id),
                    MinPrice = SqlFunc.AggregateMin(a.CurrentPrice),
                    AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice), 0)
                });

            return quary.ToList();
        }

    }
}