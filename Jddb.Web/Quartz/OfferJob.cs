using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonHelper;
using Jddb.Core.Extend;
using Jddb.Core.Model;
using Jddb.Service;
using mtTools;
using Quartz;
using Quartz.Util;

namespace Jddb.Web.Quartz
{
    /// <summary>
    /// 竞价任务
    /// </summary>
    public class OfferJob : IJob
    {
        private WebServer _webServer =new WebServer();
        private BaseServer<JobOffer> _jobOfferServer =new BaseServer<JobOffer>();


        public async Task Execute(IJobExecutionContext context)
        {
            var joboffer = context.JobDetail.JobDataMap.GetString("Json").ToObject<JobOffer>();

            await OfferAuctionId(joboffer);
        }


        /// <summary>
        /// 出价
        /// </summary>
        /// <param name="jobOffer"></param>
        /// <returns></returns>
        public async Task OfferAuctionId(JobOffer jobOffer)
        {
            // 获取Thor
            var redisKey = MyKeys.RedisUserInfo(jobOffer.UserId);
            var userThor = RedisHelper.Get<UserThor>(redisKey);

            // 用户Thor失效，不执行任务
            if (!userThor.Status)
                return;

            // 加价竞拍
            if (jobOffer.OfferType == Enums.OfferTypeEnum.AddPrice)
            {
                var currentInfo = _webServer.GetCurrentPrice(jobOffer.AuctionId);
                var op = currentInfo.CurrentPrice + jobOffer.SpanPrice;
                jobOffer.OfferPrice = op > jobOffer.MaxPrice ? jobOffer.MaxPrice : op;
            }
            // 一口价竞拍
            else
            {
                jobOffer.OfferPrice = jobOffer.MaxPrice;
            }
            var res = _webServer.Offer(jobOffer.AuctionId, jobOffer.OfferPrice, userThor.Thor);

            // 保存出价记录
            jobOffer.OfferTime=DateTime.Now;
            jobOffer.Msg = res.ToJson();
            await _jobOfferServer.AddAsync(jobOffer);

            // thor过期提醒
            if (res.StatusCode == 501)
            {
                MailHelper.SendEmail(userThor.Mail, "京东夺宝登录失效提醒", "您好，您的Thor已失效，请重新设置。");
                // 设置状态，停止任务
                userThor.Status = false;
                RedisHelper.Set(redisKey, userThor);
            }

            // 单次竞拍，中拍后删除redis任务
            if (jobOffer.JobType == Enums.JobTypeEnum.SingleUsed && res.IsSuccess)
            {
                // 判断是否中拍
                var records = _webServer.GetUserRecords(userThor.Thor).Where(o=>o.Status==BidTypeEnum.NotPay);
                if (records.Any(o => o.AuctionId == jobOffer.AuctionId))
                {
                    var userJobs = RedisHelper.Get<List<JobOffer>>(MyKeys.RedisJobUser(jobOffer.UserId));
                    userJobs.RemoveAll(o => o.UsedNo == jobOffer.UsedNo);

                    // 发送提醒邮件
                    if(!userThor.Mail.IsNullOrWhiteSpace())
                    {
                        var body = $@"
<br>商品编号：<a href='https://paipai.jd.com/auction-detail/{jobOffer.AuctionId}'>{jobOffer.AuctionId}</a>
<br>商品名称：<a href='https://paipai.jd.com/auction-detail/{jobOffer.AuctionId}'>{jobOffer.AuctionName}</a>
<br>中拍价格：{jobOffer.OfferPrice}
<br>中拍时间：{jobOffer.OfferTime}";
                        MailHelper.SendEmail(userThor.Mail, "京东夺宝中拍提醒", body);
                    }
                }
                
            }

        }
    }
}