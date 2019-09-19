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

        ///// <summary>
        ///// 计算出商品库的价格信息
        ///// </summary>
        ///// <param name="where"></param>
        ///// <returns></returns>
        //public async Task<List<PriceCache>> CalPriceCache(Expression<Func<AuctionInfo, bool>> where=null, bool Async = true)
        //{
        //    var quary = Db.Queryable<AuctionInfo>().GroupBy(a => new { a.UsedNo, a.ProductName, a.Quality })
        //        .Where(a => a.HasBidRecord)
        //        .WhereIF(where!=null,where)
        //        .Select(a => new PriceCache()
        //        {
        //            UsedNo = a.UsedNo,
        //            ProductName = a.ProductName,
        //            PrimaryPic = SqlFunc.AggregateMax(a.PrimaryPic),
        //            Quality = a.Quality,
        //            Count = SqlFunc.AggregateCount(a.Id),
        //            MinPrice = SqlFunc.AggregateMin(a.CurrentPrice),
        //            AvgPrice = SqlSugarExternal.SqlRound(SqlFunc.AggregateAvg(a.CurrentPrice), 0)
        //        });

        //    return quary.ToList();
        //}

        /// <summary>
        /// 下拉菜单搜索商品
        /// </summary>
        /// <param name="keyword">关键词，用空格分隔</param>
        /// <param name="limit">条数，默认10</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> UsedNoSelectOptions(string keyword, int limit = 10)
        {
            keyword = keyword.Replace(" ", "%");
            var quary = Db.Queryable<AuctionInfo>().GroupBy(a => new {a.UsedNo, a.ProductName, a.Quality})
                .Where(o => o.ProductName.Contains(keyword))
                .OrderBy(o => SqlFunc.AggregateCount(o.Id), OrderByType.Desc)
                .Take(limit)
                .Select(o => new AuctionInfo
                {
                    UsedNo = o.UsedNo,
                    ProductName = $"【{o.Quality}】{o.ProductName}"
                })
                .ToList();
            var dict = quary.ToDictionary(o => o.UsedNo, o => o.ProductName);
            return dict;
        }

    }
}