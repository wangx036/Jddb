using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace mtTools
{
    /// <summary>
    /// 邮件发送工具类
    /// </summary>
    public static class MailHelper
    {
        public static void SendEmail(string toEmail, string Title, string bodytxt)
        {
            //以下stmp服务器及用户名密码保证长期有效
            string mtEmailName = "夺宝提醒";
            string mtEmailSmtp = "smtp.163.com";
            string mtEmailAddress = "wangx036@163.com";
            string mtEmailPassword = "163.wxpassword";
            string mtSqm = "163sqm";

            MimeMessage message = new MimeMessage();
            //发件人
            message.From.Add(new MailboxAddress(mtEmailName, mtEmailAddress));
            //收件人
            message.To.Add(new MailboxAddress( toEmail));
            //标题
            message.Subject = Title;
            //产生一个支持HTml 的TextPart
            TextPart body = new TextPart(TextFormat.Html)
            {
                Text = bodytxt
            };

            //创建Multipart添加附件
            Multipart multipart = new Multipart("mixed");
            multipart.Add(body);
            //附件
            //string absolutePath = @"F:\桌面\新建文件夹\mysql1.sql";
            //MimePart attachment = new MimePart()
            //{
            //    //读取文件，只能用绝对路径
            //    ContentObject = new ContentObject(File.OpenRead(absolutePath), ContentEncoding.Default),
            //    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            //    ContentTransferEncoding = ContentEncoding.Base64,
            //    //文件名字
            //    FileName = Path.GetFileName(absolutePath)
            //};
            //multipart.Add(attachment);

            //正文内容，发送
            message.Body = multipart;
            //message.Body = body;
            using (SmtpClient client = new SmtpClient())
            {
                //Smtp服务器
                client.Connect(mtEmailSmtp, 587, true);
                //登录，发送
                //特别说明，对于服务器端的中文相应，Exception中有编码问题，显示乱码了
                client.Authenticate(mtEmailAddress, mtSqm);

                client.Send(message);
                //断开
                client.Disconnect(true);
            }
        }


        ///// <summary>
        ///// 发送邮件
        ///// </summary>
        ///// <param name="toEmail">要发送的邮件地址</param>
        ///// <param name="Title">邮件标题</param>
        ///// <param name="body">邮件内容</param>
        ///// <param name="ccEmail">抄送地址</param>
        ///// <param name="AttPath">附件地址(如：F:\\dir.txt)</param>
        ///// <returns></returns>
        //public static bool SendEmail(string toEmail, string Title, string body, List<string> ccEmailList = null, List<string> AttPathList = null)
        //{
        //    return SendEmail(new List<string>() {toEmail}, Title, body, ccEmailList, AttPathList);
        //}

        //public static bool SendEmail(List<string> toEmailList, string Title, string body, List<string> ccEmailList = null, List<string> AttPathList = null)
        //{
        //    try
        //    {
        //        //以下stmp服务器及用户名密码保证长期有效
        //        string mtEmailName = "夺宝提醒";
        //        string mtEmailSmtp = "smtp.163.com";
        //        string mtEmailAddress = "wangx036@163.com";
        //        string mtEmailPassword = "163.wxpassword";


        //        //声明一个Mail对象
        //        MailMessage mymail = new MailMessage();
        //        //发件人地址：如是自己，在此输入自己的邮箱
        //        mymail.From = new MailAddress(mtEmailAddress, mtEmailName);
        //        //收件人地址
        //        if (toEmailList != null && toEmailList.Count > 0)
        //        {
        //            foreach (string toEmail in toEmailList)
        //            {
        //                mymail.To.Add(new MailAddress(toEmail));
        //            }
        //        }
        //        //邮件主题
        //        mymail.Subject = Title;
        //        //邮件标题编码
        //        mymail.SubjectEncoding = System.Text.Encoding.UTF8;
        //        //发送邮件的内容
        //        mymail.Body = body;
        //        //邮件内容编码
        //        mymail.BodyEncoding = System.Text.Encoding.UTF8;
        //        //添加附件
        //        if (AttPathList != null && AttPathList.Count > 0)
        //        {
        //            foreach (string AttPath in AttPathList)
        //            {
        //                Attachment myfiles = new Attachment(AttPath);
        //                mymail.Attachments.Add(myfiles);
        //            }
        //        }
        //        //抄送到其他邮箱
        //        if (ccEmailList != null && ccEmailList.Count > 0)
        //        {
        //            foreach (string ccEmail in ccEmailList)
        //            {
        //                mymail.CC.Add(new MailAddress(ccEmail));
        //            }
        //        }
        //        //是否是HTML邮件
        //        mymail.IsBodyHtml = true;
        //        //邮件优先级
        //        mymail.Priority = MailPriority.High;
        //        //创建一个邮件服务器类
        //        SmtpClient myclient = new SmtpClient();
        //        myclient.Host = mtEmailSmtp;
        //        //SMTP服务端口
        //        myclient.Port = 25;
        //        //验证登录
        //        myclient.Credentials = new NetworkCredential(mtEmailAddress, mtEmailPassword);
        //        myclient.Send(mymail);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

    }
}
