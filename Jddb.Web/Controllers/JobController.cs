using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonHelper;
using CommonHelper.Filter;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using Jddb.Web.Quartz;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Util;

namespace Jddb.Web.Controllers
{

    /// <summary>
    /// 竞价
    /// </summary>
    [JwtAuthorize(Roles = "Admin")]
    public class JobController : BaseController
    {

        private SchedulerCenter _schedulerCenter;
        private AuctionService _auctionService;
        private BaseServer<JobOffer> _jobOfferServer;
        private BaseServer<SysAccount> _accountServer;

        public JobController(SchedulerCenter schedulerCenter,AuctionService auctionService,BaseServer<JobOffer> jobOfferServer,BaseServer<SysAccount> accountServer)
        {
            _schedulerCenter = schedulerCenter;
            _auctionService = auctionService;
            _jobOfferServer = jobOfferServer;
            _accountServer = accountServer;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ApiResult<string>> Add(JobOffer job)
        {
            // JobGroup 为用户id
            var loguser = LoginUser();
            job.JobGroup = MyKeys.QuartzGroup(loguser.Id);
            job.UserId = loguser.Id;
            

            var redisKey = MyKeys.RedisJobUser(loguser.Id);
            var userjobs = RedisHelper.Get<List<JobOffer>>(redisKey) ?? new List<JobOffer>();

            // 判断任务数剩余
            var jobs = await _schedulerCenter.GetAllJobByGroupAsync(job.JobGroup);
            var auctionJobCount = jobs.JobInfoList.Count(o => o.Name.StartsWith(MyKeys.QuartzAuctionId()));
            var redisJobCount = userjobs.Count;
            var user = await _accountServer.GetModelAsync(a => a.Id == loguser.Id);
            if (user.LimitCount < auctionJobCount + redisJobCount)
                return await ErrorTask<string>($"任务到达上限({user.LimitCount}条)");


            // 如果是商品id单次竞拍，直接加任务中，不记录
            if (job.JobType == Enums.JobTypeEnum.SingleAuction)
            {
                job.JobName =MyKeys.QuartzAuctionId(job.AuctionId);
                job.OfferTime = job.EndTime.ToDateTime();
                return await _schedulerCenter.AddScheduleJobAsync(job);
            }
            // 商品编号竞拍，加redis，每次获取商品列表时，匹配加任务
            // 相关代码 CrawlJob.cs 中
            else
            {
                //job.JobName = $"usedno{job.UsedNo}";
                userjobs.RemoveAll(o => o.UsedNo == job.UsedNo);
                userjobs.Add(job);

                RedisHelper.Set(redisKey, userjobs);
                return await SuccessTask<string>("已添加到任务库中");
            }
        }

        /// <summary>
        /// 修改任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost("update")]
        public async Task<ApiResult<string>> Update(JobOffer job)
        {
            var loguser = LoginUser();
            var redisKey = MyKeys.RedisJobUser(loguser.Id);
            if(job.UserId!=loguser.Id)
                return await ErrorTask<string>("用户不一致，请重新登录");
            if (!RedisHelper.Exists(redisKey))
                return await ErrorTask<string>("任务不存在");

            // 修改redis
            var userjobs = RedisHelper.Get<List<JobOffer>>(redisKey) ?? new List<JobOffer>();
            userjobs.RemoveAll(o => o.UsedNo == job.UsedNo);
            userjobs.Add(job);
            RedisHelper.Set(redisKey, userjobs);

            // 修改当前队列中的quartz任务
            var quartzJob = await _schedulerCenter.GetAllJobByGroupAsync(job.JobGroup);
            var jobInfos = quartzJob.JobInfoList.Where(o => o.Name.StartsWith(MyKeys.QuartzUsedNo(job.UsedNo)))
                .ToList();
            var jobStr = job.ToJson();
            foreach (var item in jobInfos)
            {
                // 先删除修改前的任务
                await _schedulerCenter.StopOrDelScheduleJobAsync(quartzJob.GroupName, item.Name, true);
                // 再添加修改后的任务
                var newJob = jobStr.ToObject<JobOffer>();
                newJob.AuctionId = int.Parse(item.Name.Substring(MyKeys.QuartzUsedNo(quartzJob.GroupName, 0).Length - 1,
                    item.Name.Length - MyKeys.QuartzUsedNo(quartzJob.GroupName, 0).Length + 1));
                await _schedulerCenter.AddScheduleJobAsync(newJob);
            }

            return await SuccessTask<string>();
        }

        /// <summary>
        /// 删除任务（同时删除redis和quartz）
        /// </summary>
        /// <param name="usedno">商品编码</param>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<ApiResult<string>> Delete(string usedno)
        {
            var result = Success<string>();
            // 删除redis
            DeleteRedisJob(usedno);
            // 删除quartz
            var keys = await _schedulerCenter.GetAllKeysByGroup(MyKeys.QuartzGroup(LoginUser().Id));
            foreach (var key in keys)
            {
                var res = await _schedulerCenter.StopOrDelScheduleJobAsync(key.Group, key.Name, true);
                if (res.IsSuccess)
                    result = res;
            }

            return await Task.Run(() => result);
        }

        /// <summary>
        /// 获取我的任务库（redis）
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRedisJobs")]
        public async Task<ApiResult<List<UsedAuction>>> GetMyJobs()
        {
            var redisKey = MyKeys.RedisJobUser(LoginUser().Id);
            var userJobs = RedisHelper.Get<List<JobOffer>>(redisKey);
            if (userJobs == null || !userJobs.Any())
                return await SuccessTask<List<UsedAuction>> ("暂无任务");

            var usedList = await _auctionService.UsedAuctions(o => userJobs.Select(j => j.UsedNo).Contains(o.UsedNo));

            foreach (var job in userJobs)
            {
                var usedItem = usedList.Find(o => o.UsedNo == job.UsedNo);
                if (job.AuctionName.IsNullOrWhiteSpace())
                    job.AuctionName = usedItem.ProductName;
                usedItem.OfferInfo = job;
            }

            //// 统计价 缓存
            //var priceTipRedis = RedisHelper.Get<List<PriceTip>>(MyKeys.RedisPriceTip);
            //foreach (var item in usedList)
            //{
            //    var tip = priceTipRedis.Find(o => o.UsedNo == item.UsedNo);
            //    item.Count = tip.Count;
            //    item.AvgPrice = tip.AvgPrice;
            //    item.MinPrice = tip.MinPrice;
            //}

            return await SuccessTask(data: usedList);
        }


        /// <summary>
        /// 获取我的任务中usedno的出价记录及目前还在执行的任务
        /// </summary>
        /// <param name="usedno">商品编码</param>
        /// <returns>item1:出价记录;item2:quartz队列中的任务</returns>
        [HttpGet("getJobDetail")]
        public async Task<ApiResult<Tuple<List<JobOffer>,List<JobInfo>>>> GetJobDetail(string usedno)
        {
            var loginuser = LoginUser();
            Expression<Func<JobOffer, bool>> whereExp;
            if (usedno.IsNullOrWhiteSpace())
                whereExp = o => o.UserId == loginuser.Id;
            else
                whereExp = o => o.UserId == loginuser.Id && o.UsedNo == usedno;
            // 获取出价记录
            var offerDetails = await _jobOfferServer.GetListAsync(whereExp,o=>o.OfferTime,OrderByEnmu.Desc);
            // 用户的quartz任务
            var jobinfo = await _schedulerCenter.GetAllJobByGroupAsync(MyKeys.QuartzGroup(loginuser.Id));
            // 挑选出usedno的quartz任务
            var jobs = jobinfo.JobInfoList.Where(o => o.Name.StartsWith(MyKeys.QuartzUsedNo(usedno))).ToList();


            var tuple = Tuple.Create(offerDetails, jobs);
            return await SuccessTask(data: tuple);
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllJobs")]
        public async Task<List<JobInfoEntity>> GetAllJobs()
        {
           return await _schedulerCenter.GetAllJobAsync();
        }

        /// <summary>
        /// 删除Redis中的任务
        /// </summary>
        /// <param name="usedno"></param>
        /// <returns></returns>
        [HttpPost("deleteRedisJob")]
        public async Task<ApiResult<string>> DeleteRedisJob(string usedno)
        {
            var loguser = LoginUser();
            var key = MyKeys.RedisJobUser(loguser.Id);
            if (usedno.IsNullOrWhiteSpace())
            {
                await RedisHelper.DelAsync(key);
            }
            else
            {
                var userjobs = RedisHelper.Get<List<JobOffer>>(key) ?? new List<JobOffer>();
                userjobs.RemoveAll(o => o.UsedNo == usedno);
                RedisHelper.Set(key, userjobs);
            }

            return await SuccessTask<string>("删除成功");
        }

        /// <summary>
        /// 删除真正执行的quartz任务
        /// </summary>
        /// <param name="auctionid"></param>
        /// <returns></returns>
        [HttpPost("deleteQuartz")]
        public async Task<ApiResult<string>> DeleteRunningJob(int? auctionid=null)
        {
            ApiResult<string> result=Success<string>();
            var keys = await _schedulerCenter.GetAllKeysByGroup(MyKeys.QuartzGroup(LoginUser().Id));

            if (auctionid != null)
                keys = keys.Where(k => k.Name.Contains(MyKeys.QuartzAuctionId((int)auctionid))).ToList();
            foreach (var key in keys)
            {
                var res = await _schedulerCenter.StopOrDelScheduleJobAsync(key.Group, key.Name, true);
                if (!res.IsSuccess)
                    result = res;
            }

            return await Task.Run(() => result);
        }

    }
}