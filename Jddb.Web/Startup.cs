using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommonHelper;
using CommonHelper.Cache;
using CommonHelper.Filter;
using Jddb.Web.Quartz;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;
using NLog.Web;
using Quartz;
using Quartz.Impl;
using Swashbuckle.AspNetCore.Swagger;

namespace Jddb.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //自定注册
            AddAssembly(services, "Jddb.Service");
            services.AddSingleton<SchedulerCenter>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            #region JWT认证
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
                {
                   
                })
            .AddJwtBearer(JwtAuthorizeAttribute.JwtAuthenticationScheme, o =>
            {
                var jwtConfig = new JwtAuthConfigModel();
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateAudience = true,//是否验证Audience
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    ValidateLifetime = true,//是否验证超时  当设置exp和nbf时有效 同时启用ClockSkew 
                    ClockSkew = TimeSpan.FromSeconds(30),//注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
                    ValidAudience = jwtConfig.Audience,//Audience
                    ValidIssuer = jwtConfig.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtAuth:SecurityKey"]))//拿到SecurityKey
                };
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // 如果过期，则把<是否过期>添加到，返回头信息中
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion

            #region 授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy("App", policy => policy.RequireRole("App").Build());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
                options.AddPolicy("All", policy => policy.RequireRole("Admin,App").Build());
            });
            #endregion

            #region 缓存
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            RedisHelper.Initialization(new CSRedis.CSRedisClient(Configuration["Cache:Configuration"]));
            
            #endregion

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Jddb API", Version = "v1" });
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Jddb.Web.xml");
                var entityXmlPath = Path.Combine(basePath, "Jddb.Core.xml");
                options.IncludeXmlComments(xmlPath, true);
                options.IncludeXmlComments(entityXmlPath);

                var security = new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } }, };
                //添加一个必须的全局安全信息，和AddSecurityDefinition方法指定的方案名称要一致，这里是Bearer。
                options.AddSecurityRequirement(security);
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                    //jwt默认的参数名称
                    Name = "Authorization",
                    //jwt默认存放Authorization信息的位置(请求头中)
                    In = "header",
                    Type = "apiKey"
                });
            });

            #endregion

            #region json格式

            services.AddMvc().AddJsonOptions(option =>
            {
                ////配置大小写问题，默认是首字母小写
                //option.SerializerSettings.ContractResolver =
                //    new Newtonsoft.Json.Serialization.DefaultContractResolver();
                //配置序列化时时间格式为yyyy-MM-dd HH:mm:ss
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            #endregion

            //(new SchedulerCenter()).SetCrawlJob();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    //自定义异常处理
            //    app.UseMiddleware<ExceptionFilter>();
            //}

            //自定义异常处理
            app.UseMiddleware<ExceptionFilter>();

            //Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jddb API V1");
            });

            //添加NLog  
            loggerFactory.AddNLog();
            //读取Nlog配置文件 
            env.ConfigureNLog("nlog.config");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //(new CronJob()).InsertAuctionAndBidRecord();
        }


        /// <summary>  
        /// 自动注册服务——获取程序集中的实现类对应的多个接口
        /// </summary>
        /// <param name="services">服务集合</param>  
        /// <param name="assemblyName">程序集名称</param>
        public void AddAssembly(IServiceCollection services, string assemblyName)
        {
            if (!String.IsNullOrEmpty(assemblyName))
            {
                Assembly assembly = Assembly.Load(assemblyName);
                List<Type> ts = assembly.GetTypes().Where(u => u.IsClass &&u.IsPublic).ToList();
                foreach (var item in ts)
                {
                    services.AddTransient(item);
                    //var interfaceType = item.GetInterfaces();
                    //if (interfaceType.Length == 1)
                    //{
                    //    services.AddTransient(interfaceType[0], item);
                    //}
                    //if (interfaceType.Length > 1)
                    //{
                    //    services.AddTransient(interfaceType[1], item);
                    //}
                }
            }
        }
    }
}
