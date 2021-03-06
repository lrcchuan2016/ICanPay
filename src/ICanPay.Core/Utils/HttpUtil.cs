﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ICanPay.Core
{
    /// <summary>
    /// Http工具类
    /// </summary>
    public class HttpUtil
    {
        #region 属性

        private static IHttpContextAccessor HttpContextAccessor;

        /// <summary>
        /// 当前上下文
        /// </summary>
        public static HttpContext Current => HttpContextAccessor.HttpContext;

        /// <summary>
        /// 本地IP
        /// </summary>
        public static IPAddress LocalIpAddress => Current.Connection.LocalIpAddress;

        /// <summary>
        /// 客户端IP
        /// </summary>
        public static IPAddress RemoteIpAddress => Current.Connection.RemoteIpAddress;

        /// <summary>
        /// 用户代理
        /// </summary>
        public static string UserAgent => Current.Request.Headers["UserAgent"];

        /// <summary>
        /// 请求类型
        /// </summary>
        public static string RequestType => Current.Request.Headers["RequestType"];

        /// <summary>
        /// 内容类型
        /// </summary>
        public static string ContentType => Current.Request.ContentType;

        /// <summary>
        /// 参数
        /// </summary>
        public static string QueryString => Current.Request.QueryString.ToString();

        /// <summary>
        /// 表单
        /// </summary>
        public static IFormCollection Form => Current.Request.Form;

        /// <summary>
        /// 请求体
        /// </summary>
        public static Stream Body => Current.Request.Body;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        internal static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 跳转到指定链接
        /// </summary>
        /// <param name="url">链接</param>
        public static void Redirect(string url)
        {
            Current.Response.Redirect(url);
        }

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="text">内容</param>
        public static void Write(string text)
        {
            Current.Response.ContentType = "text/html;charset=utf-8";
            Current.Response.WriteAsync(text).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 读取网页，返回网页内容
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static string ReadPage(string url)
        {
            return ReadPage(url, Encoding.UTF8);
        }

        /// <summary>
        /// 异步读取网页，返回网页内容
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static async Task<string> ReadPageAsync(string url)
        {
            return await ReadPageAsync(url, Encoding.UTF8);
        }

        /// <summary>
        /// 读取网页，返回网页内容
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string ReadPage(string url, Encoding encoding)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        if (reader != null)
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                request.Abort();
            }

            return string.Empty;
        }

        /// <summary>
        /// 读取网页，返回网页内容
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static async Task<string> ReadPageAsync(string url, Encoding encoding)
        {
            return await Task.Run(() => ReadPage(url, encoding));
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static string Post(string url, string data)
        {
            byte[] dataByte = Encoding.UTF8.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.ContentLength = dataByte.Length;

            try
            {
                using (Stream outStream = request.GetRequestStream())
                {
                    outStream.Write(dataByte, 0, dataByte.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        if (reader != null)
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                request.Abort();
            }

            return string.Empty;
        }

        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string data)
        {
            return await Task.Run(() => Post(url, data));
        }

        #endregion
    }
}
