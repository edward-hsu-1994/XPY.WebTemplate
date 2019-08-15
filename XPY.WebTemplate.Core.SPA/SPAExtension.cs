using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSwag.Generation.Processors.Security;
using System;
using System.Net;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// SPA擴充
    /// </summary>
    public static class SPAExtension {
        /// <summary>
        /// 加入SPA
        /// </summary>
        /// <param name="services">DI服務容器</param>
        /// <returns>DI服務容器</returns>
        public static void AddSpa(this IServiceCollection services) {
            // 設定SPA根目錄
            services.AddSpaStaticFiles(options => {
                options.RootPath = "./wwwroot";
            });
        }

        /// <summary>
        /// 使用SPA
        /// </summary>
        /// <param name="app">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static void UseSpa(this IApplicationBuilder app) {
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
