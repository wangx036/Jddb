using System;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core;
using Jddb.Core.Model;
using SqlSugar;

namespace Jddb.Service
{
    public class PriceCacheServer:BaseServer<PriceCache>
    {
        /// <summary>
        /// 设置价格缓存
        /// </summary>
        /// <returns></returns>
        public async Task SetPriceCache()
        {
            Db.Ado.ExecuteCommand($"truncate table pricecache;");
            var insertSql =
                @"insert into pricecache (UsedNo,ProductName,PrimaryPic,Quality,Count,MinPrice,AvgPrice)
select UsedNo,ProductName,min(PrimaryPic) PrimaryPic,Quality,Count(*) Count,min(CurrentPrice) MinPrice,avg(CurrentPrice) AvgPrice
from auctioninfo where hasbidrecord=1
group by usedno,ProductName,Quality";
            var count = Db.Ado.ExecuteCommand(insertSql);

        }

    }
}