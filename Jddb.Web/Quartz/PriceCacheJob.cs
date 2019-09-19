using System.Threading.Tasks;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using Quartz;

namespace Jddb.Web.Quartz
{
    /// <summary>
    /// 缓存 价格信息（平均价、最小价）
    /// </summary>
    public class PriceCacheJob:IJob
    {
        private PriceCacheServer _priceServer=new PriceCacheServer();

        public async Task Execute(IJobExecutionContext context)
        {
            _priceServer.SetPriceCache();
        }



    }
}