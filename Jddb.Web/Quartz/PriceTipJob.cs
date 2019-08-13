using System.Threading.Tasks;
using Jddb.Core.Extend;
using Jddb.Service;
using Quartz;

namespace Jddb.Web.Quartz
{
    /// <summary>
    /// 缓存 价格信息（平均价、最小价）
    /// </summary>
    public class PriceTipJob:IJob
    {
        private AuctionService _auctionService=new AuctionService();

        public async Task Execute(IJobExecutionContext context)
        {
            var list = await _auctionService.PriceTipsAsync();
            RedisHelper.Set(MyKeys.RedisPriceTip, list);
        }



    }
}