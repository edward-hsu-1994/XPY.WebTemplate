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

namespace XPY.WebTemplate {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContextPool<DbContext>(options => {
                options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            });

            // 日誌紀錄器
            services.AddLogging();

            // 支援DI取得HttpContext
            services.AddHttpContextAccessor();

            // JWT支援
            services.AddJwtAuthentication(
                issuer: "<YOUR ISSUER>",
                audience: "<YOUR AUDIENCE>",
                secureKey: "<YOUR SECURE KEY>");

            // Swagger產生器
            services.AddSwaggerDocument(config => {
                config.Title = "<YOUR TITLE>";
                config.Description = "<YOUR DESCRIPTION>";
                config.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();

                // ref: https://github.com/RSuter/NSwag/issues/869
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("apiKey"));
                /*config.OperationProcessors.Add(new AuthorizeOperationProcessor());
                config.OperationProcessors.Add(new OptionParamProcessor());
                config.OperationProcessors.Add(new StringEnumParamProcessor());
                config.OperationProcessors.Add(new FixFormFileParamProcessor());
                config.OperationProcessors.Add(new DefaultEnumParamProcessor());
                config.OperationProcessors.Add(new ConsumesAttributeProcessor());*/
                config.DocumentProcessors.Add(new SecurityDefinitionAppender(
                    "apiKey",
                    new string[0],
                    new NSwag.OpenApiSecurityScheme() {
                        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Description = "JWT(Bearer) 存取權杖"
                    }));
            });

            // MiniProfiler支援
            services.AddMiniProfiler(o => {
                o.RouteBasePath = "/profiler";
            })
            .AddEntityFramework();

            // 客戶端裝置追蹤
            services.AddScoped<DeviceDetector>(sp => {
                var httpContext = sp.GetService<IHttpContextAccessor>().HttpContext;

                DeviceDetector detector = null;
                if (httpContext.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent)) {
                    detector = new DeviceDetector(userAgent);
                    detector.SetCache(new DictionaryCache());
                    detector.Parse();
                }

                return detector;
            });

            // 加入IP轉國家支援
            services.AddIP2Country();

            // Hangfire排程支援
            services.AddHangfire(config => {
                // 設定使用MemoryStorage
                config.UseMemoryStorage();

                // 支援Console
                config.UseConsole();
            });

            // 設定SPA根目錄
            services.AddSpaStaticFiles(options => {
                options.RootPath = "./wwwroot";
            });

            // 中文編碼問題修正
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            services.AddMvc()
                .AddJsonOptions(options => {
                    // JSON序列化忽略循環問題
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    // 列舉
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseMiniProfiler();
            }

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
            // 加入Hangfire伺服器
            app.UseHangfireServer();

            // 加入Hangfire控制面板
            app.UseHangfireDashboard(
                pathMatch: "/hangfire",
                options: new DashboardOptions() { // 使用自訂的認證過濾器
                    Authorization = new[] { new HangfireAuthorizeFilter() }
                }
            );
            #endregion

            #region Swagger UI            
            app.UseOpenApi();
            app.UseSwaggerUi3();
            #endregion

            // 使用認證
            app.UseAuthentication();

            app.UseMvc();

            // 使用靜態檔案
            app.UseStaticFiles();

            // 使用SPA
            app.UseSpaStaticFiles();

            // SPA例外處理
            app.Use(async (context, next) => {
                try {
                    await next();
                } catch (Exception e) {
                    if (e is InvalidOperationException && e.Message.Contains("/index.html")) {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
            });

            // SPA設定
            app.UseSpa(c => { });
        }
    }
}
