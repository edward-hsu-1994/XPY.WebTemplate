using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using XPY.WebTemplate.Core.Authorization;

namespace XPY.WebTemplate {
    public class Program {
        public static void Main(string[] args) {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try {
                logger.Debug("init main");
                CreateWebHostBuilder(args).Build().Run();
            } catch (Exception ex) {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            } finally {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IConfigurationRoot ReadFromAppSettings() {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var config = ReadFromAppSettings();

            var builder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();


            if (config.GetValue<bool>("Logging:LogFile")) {
                // 加入檔案型Log
                builder
                    .ConfigureLogging(logging => {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    })
                    .UseNLog();
            }


            if (config.GetValue<bool>("Sentry:Enable")) {
                builder = builder.UseSentry(config.GetValue<string>("Sentry:DSN"));
            }

            return builder;
        }
    }
}
