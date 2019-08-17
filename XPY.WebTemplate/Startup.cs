using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Firewall;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using NSwag.Generation.Processors.Security;
using XPY.WebTemplate.Core.Authorization;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using DeviceDetectorNET.Cache;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Console;
using IP2Country;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSwag;
using System.Text;
using FluentValidation.AspNetCore;
using XPY.WebTemplate.Models.Validators;

namespace XPY.WebTemplate {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // DbContext
            services.AddDbContextPool<DbContext>(options => {
                options.ConfigureWarnings(warnings =>
                    warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            });

            // 日誌紀錄器
            services.AddLogging();

            // 支援DI取得HttpContext
            services.AddHttpContextAccessor();

            // JWT支援
            services.AddJwtAuthentication(
                issuer: Configuration.GetValue<string>("JWT:Issuer"),
                audience: Configuration.GetValue<string>("JWT:Audience"),
                secureKey: Configuration.GetValue<string>("JWT:SecureKey"));

            // Swagger產生器
            if (Configuration.GetValue<bool>("Swagger:Enable")) {
                services.AddNSwag(
                    title: Configuration.GetValue<string>("Swagger:Title"),
                    description: Configuration.GetValue<string>("Swagger:Description"));
            }

            // MiniProfile
            if (Configuration.GetValue<bool>("MiniProfiler:Enable")) {
                // MiniProfiler支援
                services.AddMiniProfiler(o => {
                    o.RouteBasePath = Configuration.GetValue<string>("MiniProfiler:RouteBasePath");
                }).AddEntityFramework();
            }

            // 客戶端裝置追蹤
            if (Configuration.GetValue<bool>("DeviceDetector")) {
                services.AddDeviceDetector();
            }

            // 加入IP轉國家支援
            if (Configuration.GetValue<bool>("IP2Country")) {
                services.AddIP2Country();
            }

            // Hangfire排程支援
            if (Configuration.GetValue<bool>("Hangfire:Enable")) {
                services.AddHangfire(config => {
                    // 設定使用MemoryStorage
                    config.UseMemoryStorage();

                    // 支援Console
                    config.UseConsole();
                });
            }

            // 設定SPA根目錄
            services.AddSpa();

            // 中文編碼問題修正
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            // 加入服務
            services.AddServices();

            // 加入模型驗證器
            services.AddFluentValidation();

            // MVC
            services.AddMvc()
                .AddJsonOptions(options => {
                    // JSON序列化忽略循環問題
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    // 列舉
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation()
                .AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory) {

            if (Configuration.GetValue<bool>("Logging:LogFile")) {
                // 加入檔案型Log
                loggerFactory.AddFile("logs/{Date}.log");
            }

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            if (Configuration.GetValue<bool>("MiniProfiler:Enable")) {
                app.UseMiniProfiler();
            }

            // 轉發標頭
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.All
            });

            #region Firewall
            /*
            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromIPAddressRanges(allowedCIDRs)
                    .ExceptFromIPAddresses(allowedIPs));
            */
            #endregion

            // 回應緩衝
            app.UseResponseBuffering();

            #region Security Headers
            var policyCollection = new HeaderPolicyCollection()
                .AddFrameOptionsDeny()
                .AddXssProtectionBlock()
                .AddContentTypeOptionsNoSniff()
                .AddReferrerPolicyStrictOriginWhenCrossOrigin()
                .RemoveServerHeader();
            //.AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365)
            //.AddContentSecurityPolicy(builder => {
            //    builder.AddObjectSrc().None();
            //    builder.AddFormAction().Self();
            //    builder.AddFrameAncestors().None();
            //});

            app.UseSecurityHeaders(policyCollection);
            #endregion

            #region Hangfire
            if (Configuration.GetValue<bool>("Hangfire:Enable")) {
                // 加入Hangfire伺服器
                app.UseHangfireServer();

                // 加入Hangfire控制面板
                app.UseHangfireDashboard(
                    pathMatch: Configuration.GetValue<string>("Hangfire:PathMatch"),
                    options: new DashboardOptions() { // 使用自訂的認證過濾器
                        Authorization = new[] { new HangfireAuthorizeFilter() }
                    }
                );
            }
            #endregion

            // Swagger UI            
            if (Configuration.GetValue<bool>("Swagger:Enable")) {
                app.UseOpenApiAndSwaggerUi3();
            }

            // 使用認證
            app.UseAuthentication();

            // 使用MVC
            app.UseMvc();

            // 使用靜態檔案
            app.UseStaticFiles();

            // 使用SPA
            app.UseSpa();
        }
    }
}
