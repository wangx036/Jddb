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
        /// 获取我的任务库（redis）
        /// </summary>
        /// <returns></returns>
        [HttpGet("getRedisJobs")]
        public async Task<ApiResult<List<UsedAuction>>> GetMyJobs()
        {
            var redisKey = MyKeys.RedisJobUser(LoginUser().Id);
            var userJobs = RedisHelper.Get<List<JobOffer>>(redisKey);

            var usedList = await _auctionService.UsedAuctions(o => userJobs.Select(j => j.UsedNo).Contains(o.UsedNo));

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
        public async Task DeleteRedisJob(string usedno)
        {
            var loguser = LoginUser();
            if (usedno.IsNullOrWhiteSpace())
               await RedisHelper.DelAsync(MyKeys.RedisJobUser(loguser.Id));
            else
            {
                var redisKey =MyKeys.RedisJobUser(loguser.Id);
                var userjobs = RedisHelper.Get<List<JobOffer>>(redisKey) ?? new List<JobOffer>();
                userjobs.RemoveAll(o => o.UsedNo == usedno);
            }
        }

        /// <summary>
        /// 删除真正执行的quartz任务
        /// </summary>
        /// <param name="auctionid"></param>
        /// <returns></returns>
        [HttpPost("deleteQuartz")]
        public async Task DeleteRunningJob(int? auctionid=null)
        {
            var keys = await _schedulerCenter.GetAllKeysByGroup(MyKeys.QuartzGroup(LoginUser().Id));

            if (auctionid != null)
                keys = keys.Where(k => k.Name.Contains(MyKeys.QuartzAuctionId((int)auctionid))).ToList();
            foreach (var key in keys)
            {
                _schedulerCenter.StopOrDelScheduleJobAsync(key.Group, key.Name, true);
            }
        }

    }
}