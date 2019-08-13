using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper
{
    public class NetHelper
    {
        #region 属性

        /// <summary>
        /// IP
        /// </summary>
        public static string Ip => GetWebClientIp();
        /// <summary>
        /// IP归属地
        /// </summary>
        public static string IpAddress => GetAddress(Ip);
        /// <summary>
        /// 当前访问的URL地址
        /// </summary>
        public static string Url => GetUrl();

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取Web客户端IP。
        /// </summary>
        private static string GetWebClientIp()
        {

            HttpContextAccessor context = new HttpContextAccessor();
            var ip = context.HttpContext?.Connection.RemoteIpAddress.ToString();
            return string.IsNullOrWhiteSpace(ip) ? string.Empty : ip;
        }
        /// <summary>
        /// 获取局域网IP。
        /// </summary>
        private static string GetLanIp()
        {
            foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return string.Empty;
        } 
        /// <summary>
        /// 获得当前访问的URL地址
        /// </summary>
        /// <returns>字符串数组</returns>
        private static string GetUrl()
        {
            HttpContextAccessor context = new HttpContextAccessor();
            return context.HttpContext.Request.Path.ToString();
        }

        #endregion

        #region 共有方法

        /// <summary>
        /// 获取IP地址信息（源：淘宝IP库接口）。
        /// </summary>
        /// <param name="ip">IP</param>
        /// <returns></returns>
        public static string GetAddress(string ip)
        {
            string result = string.Empty;
            var url = "http://ip.taobao.com/service/getIpInfo.php?ip=" + ip;
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(url).ToObject<TaoBaoIpEnitiy>().GetAddress();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        #region HttpGet

        /// <summary>
        /// 使用Get方法获取字符串结果（没有加入Cookie）
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, Encoding encoding = null)
        {
            HttpClient httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(url);
            var ret = encoding.GetString(data);
            return ret;
        }
        /// <summary>
        /// Http Get 同步方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpGet(string url, Encoding encoding = null)
        {
            HttpClient httpClient = new HttpClient();
            var t = httpClient.GetByteArrayAsync(url);
            t.Wait();
            var ret = encoding.GetString(t.Result);
            return ret;
        }

        #endregion

        #region HttpPost

        /// <summary>
        /// POST 异步
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postStream"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(string url, Dictionary<string, string> formData = null, Encoding encoding = null, int timeOut = 10000)
        {

            HttpClientHandler handler = new HttpClientHandler();

            HttpClient client = new HttpClient(handler);
            MemoryStream ms = new MemoryStream();
            FillFormDataStream(formData,ms);//填充formData
            HttpContent hc = new StreamContent(ms);


            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            hc.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            hc.Headers.Add("Timeout", timeOut.ToString());
            hc.Headers.Add("KeepAlive", "true");

            var r = await client.PostAsync(url, hc);
            byte[] tmp = await r.Content.ReadAsByteArrayAsync();

            return encoding.GetString(tmp);
        }

        /// <summary>
        /// POST 同步
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postStream"></param>
        /// <param name="encoding"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string HttpPost(string url, Dictionary<string, string> formData = null, Encoding encoding = null, int timeOut = 10000)
        {

            HttpClientHandler handler = new HttpClientHandler();

            HttpClient client = new HttpClient(handler);
            MemoryStream ms = new MemoryStream();
            FillFormDataStream(formData,ms);//填充formData
            HttpContent hc = new StreamContent(ms);


            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            hc.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            hc.Headers.Add("Timeout", timeOut.ToString());
            hc.Headers.Add("KeepAlive", "true");

            var t = client.PostAsync(url, hc);
            t.Wait();
            var t2 = t.Result.Content.ReadAsByteArrayAsync();
            return encoding.GetString(t2.Result);
        }

        /// <summary>
        /// 组装QueryString的方法
        /// 参数之间用&连接，首位没有符号，如：a=1&b=2&c=3
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static string GetQueryString(Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, kv.Value);
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }
        /// <summary>
        /// 填充表单信息的Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static void FillFormDataStream(Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }
        #endregion



        #endregion


        #region 淘宝IP地址库接口模型
        /// <summary>
        /// 淘宝IP地址库接口模型。
        /// http://ip.taobao.com/instructions.php
        /// </summary>
        internal class TaoBaoIpEnitiy
        {
            /// <summary>
            /// 响应结果。
            /// </summary>
            public int code { get; set; }
            /// <summary>
            ///  响应数据。
            /// </summary>
            public IpDataEnitiy data { get; set; }

            public string GetAddress()
            {
                if (this.data == null)
                {
                    return string.Empty;
                }
                return string.Format("{0} {1} {2}", this.data.country, this.data.region, this.data.city);
            }
        }

        internal class IpDataEnitiy
        {
            /// <summary>
            /// 国家
            /// </summary>
            public string country { get; set; }
            /// <summary>
            /// 国家ID
            /// </summary>
            public string country_id { get; set; }
            /// <summary>
            /// 地区
            /// </summary>
            public string area { get; set; }
            /// <summary>
            /// 地区ID
            /// </summary>
            public string area_id { get; set; }
            /// <summary>
            /// 省份
            /// </summary>
            public string region { get; set; }
            /// <summary>
            /// 省份ID
            /// </summary>
            public string region_id { get; set; }
            /// <summary>
            /// 城市
            /// </summary>
            public string city { get; set; }
            /// <summary>
            /// 城市ID
            /// </summary>
            public string city_id { get; set; }
            /// <summary>
            /// 地区
            /// </summary>
            public string county { get; set; }
            /// <summary>
            /// 地区ID
            /// </summary>
            public string county_id { get; set; }
            /// <summary>
            /// 运营商
            /// </summary>
            public string isp { get; set; }
            /// <summary>
            /// 运营商ID
            /// </summary>
            public string isp_id { get; set; }
            /// <summary>
            /// IP
            /// </summary>
            public string ip { get; set; }
        }
        #endregion


    }
}