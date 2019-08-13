using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Model;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json.Linq;
using NLog.Fluent;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using Quartz.Util;

namespace Jddb.Web.Quartz
{
    /// <summary>
    /// 任务调度对象
    /// </summary>
    public class SchedulerCenter
    {

        private IScheduler _scheduler;
        public SchedulerCenter()
        {

        }


        /// <summary>
        /// 返回任务计划（调度器）
        /// </summary>
        /// <returns></returns>
        private IScheduler Scheduler
        {
            get
            {
                if (_scheduler != null)
                {
                    return _scheduler;
                }
                ISchedulerFactory schedulerFactory =new StdSchedulerFactory();

                _scheduler = schedulerFactory.GetScheduler().Result;
                _scheduler.Start();//默认开始调度器
                return _scheduler;
            }
        }

        /// <summary>
        /// 添加一个工作调度
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task SetCrawlJob()
        {
            var entity = new ScheduleEntity()
            {
                JobGroup = "CrawlJob",
                JobName = "CrawlJob",
                Description = "爬取数据，同时添加自动竞价任务",
                TriggerType = TriggerTypeEnum.Cron,
                BeginTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddYears(10),
                Cron = "0 0/2 8-23 * * ?" //每天8-23点，每2分钟执行一次
            };
            //检查任务是否已存在
            var jobKey = new JobKey(entity.JobName, entity.JobGroup);
            if (!await Scheduler.CheckExists(jobKey))
            {

                // 定义这个工作，并将其绑定到我们的IJob实现类                
                IJobDetail job = JobBuilder.CreateForAsync<CrawlJob>()
                    .WithDescription(entity.Description)
                    .WithIdentity(entity.JobName, entity.JobGroup)
                    .Build();
                // 创建触发器
                ITrigger trigger;
                //校验是否正确的执行周期表达式
                if (entity.TriggerType == TriggerTypeEnum.Cron) //CronExpression.IsValidExpression(entity.Cron))
                {
                    trigger = CreateCronTrigger(entity);
                }
                else
                {
                    trigger = CreateSimpleTrigger(entity);
                }

                // 告诉Quartz使用我们的触发器来安排作业
                await Scheduler.ScheduleJob(job, trigger);

            }
        }

        /// <summary>
        /// 添加一个工作调度
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ApiResult<string>> AddScheduleJobAsync(JobOffer entity)
        {
            try
            {
                //检查任务是否已存在
                var jobKey = new JobKey(entity.JobName, entity.JobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    return await RM.ErrorTask<string>("任务已存在！");
                }

                //http请求配置
                var httpDir = new Dictionary<string, string>()
                {
                    {"Json", entity.ToJson()},
                };
                // 定义这个工作，并将其绑定到我们的IJob实现类                
                IJobDetail job = JobBuilder.CreateForAsync<OfferJob>()
                    .SetJobData(new JobDataMap(httpDir))
                    .WithIdentity(entity.JobName, entity.JobGroup)
                    .Build();
                // 创建触发器
                ITrigger trigger = CreateMyOfferTrigger(entity);

                // 告诉Quartz使用我们的触发器来安排作业
                await Scheduler.ScheduleJob(job, trigger);
                return await RM.SuccessTask<string>("任务添加成功！");
            }
            catch (Exception ex)
            {
                return await RM.ErrorTask<string>("添加任务计划失败！", ex.Message);
            }
        }

        /// <summary>
        /// 暂停/删除 指定的计划
        /// </summary>
        /// <param name="jobGroup">任务分组</param>
        /// <param name="jobName">任务名称</param>
        /// <param name="isDelete">停止并删除任务</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> StopOrDelScheduleJobAsync(string jobGroup, string jobName, bool isDelete = false)
        {
            try
            {
                await Scheduler.PauseJob(new JobKey(jobName, jobGroup));
                if (isDelete)
                {
                    await Scheduler.DeleteJob(new JobKey(jobName, jobGroup));
                    return RM.Success<string>("删除任务计划成功！");
                }
                else
                {
                    return RM.Success<string>("停止任务计划成功！");
                }

            }
            catch (Exception ex)
            {
                return RM.Error<string>("停止任务计划失败！", ex.Message);
            }
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="jobGroup">任务分组</param>
        public async Task<ApiResult<string>> ResumeJobAsync(string jobGroup, string jobName)
        {
            try
            {
                //检查任务是否存在
                var jobKey = new JobKey(jobName, jobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    //任务已经存在则暂停任务
                    await Scheduler.ResumeJob(jobKey);
                    return RM.Success<string>($"任务“{jobName}”恢复运行！");
                }
                else
                {
                    return RM.Success<string>("任务不存在！");
                }
            }
            catch (Exception ex)
            {
                return RM.Error<string>("恢复任务计划失败！", ex.Message);
            }
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public async Task<ScheduleEntity> QueryJobAsync(string jobGroup, string jobName)
        {
            var entity = new ScheduleEntity();
            var jobKey = new JobKey(jobName, jobGroup);
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
            var triggers = triggersList.AsEnumerable().FirstOrDefault();
            var intervalSeconds = (triggers as SimpleTriggerImpl)?.RepeatInterval.TotalSeconds;
            entity.RequestUrl = jobDetail.JobDataMap.GetString(Constant.REQUESTURL);
            entity.BeginTime = triggers.StartTimeUtc.LocalDateTime;
            entity.EndTime = triggers.EndTimeUtc?.LocalDateTime;
            entity.IntervalSecond = intervalSeconds.HasValue ? Convert.ToInt32(intervalSeconds.Value) : 0;
            entity.JobGroup = jobGroup;
            entity.JobName = jobName;
            entity.Cron = (triggers as CronTriggerImpl)?.CronExpressionString;
            entity.RunTimes = (triggers as SimpleTriggerImpl)?.RepeatCount;
            entity.TriggerType = triggers is SimpleTriggerImpl ? TriggerTypeEnum.Simple : TriggerTypeEnum.Cron;
            entity.RequestType = (RequestTypeEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.REQUESTTYPE));
            entity.RequestParameters = jobDetail.JobDataMap.GetString(Constant.REQUESTPARAMETERS);
            entity.Headers = jobDetail.JobDataMap.GetString(Constant.HEADERS);
            entity.MailMessage = (MailMessageEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.MAILMESSAGE) ?? "0");
            entity.Description = jobDetail.Description;
            return entity;
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public async Task<bool> TriggerJobAsync(JobKey jobKey)
        {
            await Scheduler.TriggerJob(jobKey);
            return true;
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<List<string>> GetJobLogsAsync(JobKey jobKey)
        {
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            return jobDetail.JobDataMap[Constant.LOGLIST] as List<string>;
        }

        /// <summary>
        /// 获取所有Job（详情信息 - 初始化页面调用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInfoEntity>> GetAllJobAsync()
        {
            List<JobKey> jboKeyList = new List<JobKey>();
            List<JobInfoEntity> jobInfoList = new List<JobInfoEntity>();
            var groupNames = await Scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobInfoEntity() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await Scheduler.GetJobDetail(jobKey);
                var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                var interval = string.Empty;
                if (triggers is SimpleTriggerImpl)
                    interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                else
                    interval = (triggers as CronTriggerImpl)?.CronExpressionString;

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        jobInfo.JobInfoList.Add(new JobInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.EXCEPTION),
                            RequestUrl = jobDetail.JobDataMap.GetString(Constant.REQUESTURL),
                            TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            BeginTime = triggers.StartTimeUtc.LocalDateTime,
                            Interval = interval,
                            EndTime = triggers.EndTimeUtc?.LocalDateTime,
                            Description = jobDetail.Description
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 获取单个任务信息
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<JobInfo> GetJobAsync(JobKey jobKey)
        {
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
            var triggers = triggersList.AsEnumerable().FirstOrDefault();

            var interval = string.Empty;
            if (triggers is SimpleTriggerImpl)
                interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
            else
                interval = (triggers as CronTriggerImpl)?.CronExpressionString;

            var job = new JobInfo()
            {
                Name = jobKey.Name,
                LastErrMsg = jobDetail.JobDataMap.GetString(Constant.EXCEPTION),
                RequestUrl = jobDetail.JobDataMap.GetString(Constant.REQUESTURL),
                TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                BeginTime = triggers.StartTimeUtc.LocalDateTime,
                Interval = interval,
                EndTime = triggers.EndTimeUtc?.LocalDateTime,
                Description = jobDetail.Description
            };
            return job;
        }

        /// <summary>
        /// 获取group中的所有jobkey
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<List<JobKey>> GetAllKeysByGroup(string groupName)
        {
            List<JobKey> jboKeyList = new List<JobKey>();
            jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
            return jboKeyList;
        }

        /// <summary>
        /// 获取单个组所有Job
        /// </summary>
        /// <returns></returns>
        public async Task<JobInfoEntity> GetAllJobByGroupAsync(string groupName)
        {
            var jobGroup = new JobInfoEntity() { GroupName = groupName };
            List<JobKey> jboKeyList = new List<JobKey>();
            jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await Scheduler.GetJobDetail(jobKey);
                var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                var interval = string.Empty;
                if (triggers is SimpleTriggerImpl)
                    interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                else
                    interval = (triggers as CronTriggerImpl)?.CronExpressionString;

                if (jobGroup.GroupName == jobKey.Group)
                {
                    jobGroup.JobInfoList.Add(new JobInfo()
                    {
                        Name = jobKey.Name,
                        LastErrMsg = jobDetail.JobDataMap.GetString(Constant.EXCEPTION),
                        RequestUrl = jobDetail.JobDataMap.GetString(Constant.REQUESTURL),
                        TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                        PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                        NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                        BeginTime = triggers.StartTimeUtc.LocalDateTime,
                        Interval = interval,
                        EndTime = triggers.EndTimeUtc?.LocalDateTime,
                        Description = jobDetail.Description
                    });
                }
            }
            return jobGroup;
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInfoEntity>> GetAllJobBriefInfoAsync()
        {
            List<JobKey> jboKeyList = new List<JobKey>();
            List<JobInfoEntity> jobInfoList = new List<JobInfoEntity>();
            var groupNames = await Scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobInfoEntity() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await Scheduler.GetJobDetail(jobKey);
                var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        jobInfo.JobInfoList.Add(new JobInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.EXCEPTION),
                            TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            //开启调度器
            if (Scheduler.InStandbyMode)
            {
                await Scheduler.Start();
            }
            return Scheduler.InStandbyMode;
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public async Task<bool> StopScheduleAsync()
        {
            //判断调度是否已经关闭
            if (!Scheduler.InStandbyMode)
            {
                //等待任务运行完成
                await Scheduler.Standby(); //TODO  注意：Shutdown后Start会报错，所以这里使用暂停。
            }
            return !Scheduler.InStandbyMode;
        }

        /// <summary>
        /// 创建出价触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateMyOfferTrigger(JobOffer entity)
        {
            return TriggerBuilder.Create()
                .WithIdentity(entity.JobName, entity.JobGroup)
                .StartAt(entity.OfferTime)//开始时间
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)//执行时间间隔，单位秒
                    .WithRepeatCount(1))//执行次数、默认从0开始
                .ForJob(entity.JobName, entity.JobGroup)//作业名称
                .Build();
        }

        /// <summary>
        /// 创建类型Simple的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(ScheduleEntity entity)
        {
            //作业触发器
            if (entity.RunTimes.HasValue && entity.RunTimes > 0)
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
               .EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x => x
                   .WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                   .WithRepeatCount(entity.RunTimes.Value))//执行次数、默认从0开始
                   .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }
            else
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
               .EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x => x
                   .WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                   .RepeatForever())//无限循环
                   .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }

        }

        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(ScheduleEntity entity)
        {
            // 作业触发器
            return TriggerBuilder.Create()
                   .WithIdentity(entity.JobName, entity.JobGroup)
                   .StartAt(entity.BeginTime)//开始时间
                   .EndAt(entity.EndTime)//结束时间
                   .WithCronSchedule(entity.Cron)//指定cron表达式
                   .ForJob(entity.JobName, entity.JobGroup)//作业名称
                   .Build();
        }

    }
}

