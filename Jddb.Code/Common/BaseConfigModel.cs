using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Jddb.Code.Common
{
    public class BaseConfigModel
    {
        /// <summary>
        /// 
        /// </summary>
        public static IConfiguration Configuration { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public static string ContentRootPath { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public static string WebRootPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="contentRootPath"></param>
        /// <param name="webRootPath"></param>
        public static void SetBaseConfig(IConfiguration config, string contentRootPath, string webRootPath)
        {
            Configuration = config;
            ContentRootPath = contentRootPath;
            WebRootPath = webRootPath;
        }
    }
}
