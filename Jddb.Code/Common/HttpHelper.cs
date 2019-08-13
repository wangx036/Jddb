using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jddb.Code.Common
{
    public class HttpHelper
    {
        /// <summary>
        /// 根据Url地址Get请求返回数据
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="headers">请求头</param>
        /// <returns>字符串</returns>
        public static T GetResponse<T>(string url,Dictionary<string,string> headers = null)
        {
            T result = default(T);
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            HttpResponseMessage response = null;
            try
            {
                httpClient.CancelPendingRequests();
                httpClient.DefaultRequestHeaders.Clear();

                if (headers != null)
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                Task<HttpResponseMessage> taskResponse = httpClient.GetAsync(url);
                taskResponse.Wait();
                response = taskResponse.Result;
                if (response.IsSuccessStatusCode)
                {
                    Task<System.IO.Stream> taskStream = response.Content.ReadAsStreamAsync();
                    taskStream.Wait();
                    //此处会抛出异常：不支持超时设置，对返回结果没有影响
                    System.IO.Stream dataStream = taskStream.Result;
                    System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                    string res = reader.ReadToEnd();
                    if (typeof(T).Name.Equals("String"))
                    {
                        return (T) Convert.ChangeType(res, typeof(T));
                    }
                    return JsonConvert.DeserializeObject<T>(res);
                }
                return result;
            }
            catch
            {
                return result;
            }
            finally
            {
                response?.Dispose();

                httpClient.Dispose();
            }
        }

        /// <summary>
        /// Post请求返回实体 
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求数据(json)</param>
        /// <param name="headers">请求头</param>
        /// <returns>实体</returns>
        public static T PostResponse<T>(string url, string postData, Dictionary<string, string> headers = null)
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            HttpResponseMessage response = null;
            try
            {
                httpClient.MaxResponseContentBufferSize = 256000;
                httpClient.CancelPendingRequests();
                httpClient.DefaultRequestHeaders.Clear();

                if (headers != null)
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                HttpContent httpContent = new StringContent(postData);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                T result = default(T);
                Task<HttpResponseMessage> taskResponse = httpClient.PostAsync(url, httpContent);
                taskResponse.Wait();
                response = taskResponse.Result;
                if (response.IsSuccessStatusCode)
                {
                    Task<System.IO.Stream> taskStream = response.Content.ReadAsStreamAsync();
                    taskStream.Wait();
                    System.IO.Stream dataStream = taskStream.Result;
                    System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                    string s = reader.ReadToEnd();
                    string res = reader.ReadToEnd();
                    if (typeof(T).Name.Equals("String"))
                    {
                        return (T)Convert.ChangeType(res, typeof(T));
                    }
                    result = JsonConvert.DeserializeObject<T>(s);
                }
                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }
            finally
            {
                response?.Dispose();

                httpClient.Dispose();

            }

        }

        /// <summary>
        /// Post请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求数据(json)</param>
        /// <param name="headers">请求头</param>
        /// <returns>实体</returns>
        public static string PostResponse(string url, string postData, Dictionary<string, string> headers = null)
        {
            var result = "";
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            HttpResponseMessage response = null;
            try
            {
                httpClient.MaxResponseContentBufferSize = 256000;
                httpClient.CancelPendingRequests();
                httpClient.DefaultRequestHeaders.Clear();

                if (headers != null)
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                HttpContent httpContent = new StringContent(postData);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> taskResponse = httpClient.PostAsync(url, httpContent);
                taskResponse.Wait();
                response = taskResponse.Result;
                if (response.IsSuccessStatusCode)
                {
                    Task<System.IO.Stream> taskStream = response.Content.ReadAsStreamAsync();
                    taskStream.Wait();
                    System.IO.Stream dataStream = taskStream.Result;
                    System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch
            {
                return result;
            }
            finally
            {
                response?.Dispose();

                httpClient.Dispose();

            }

        }


        /// <summary>
        /// Post请求返回实体 
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求数据(json)</param>
        /// <param name="headers">请求头</param>
        /// <returns>实体</returns>
        public static T PostResponseFormData<T>(string url, string postData, Dictionary<string, string> headers = null)
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            HttpResponseMessage response = null;
            try
            {
                httpClient.MaxResponseContentBufferSize = 256000;
                httpClient.CancelPendingRequests();
                httpClient.DefaultRequestHeaders.Clear();

                if (headers != null)
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                HttpContent httpContent = new StringContent(postData);

                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                T result = default(T);
                Task<HttpResponseMessage> taskResponse = httpClient.PostAsync(url, httpContent);
                taskResponse.Wait();
                response = taskResponse.Result;
                if (response.IsSuccessStatusCode)
                {
                    Task<System.IO.Stream> taskStream = response.Content.ReadAsStreamAsync();
                    taskStream.Wait();
                    System.IO.Stream dataStream = taskStream.Result;
                    System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                    string s = reader.ReadToEnd();
                    result = JsonConvert.DeserializeObject<T>(s);
                }
                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }
            finally
            {
                response?.Dispose();

                httpClient.Dispose();

            }

        }



    }
}