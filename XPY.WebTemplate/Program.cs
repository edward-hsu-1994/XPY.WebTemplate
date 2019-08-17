using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XPY.WebTemplate.Core.Authorization;

namespace XPY.WebTemplate {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
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

            if (config.GetValue<bool>("Sentry:Enable")) {
                builder = builder.UseSentry(config.GetValue<string>("Sentry:DSN"));
            }

            return builder;
        }
    }
}
