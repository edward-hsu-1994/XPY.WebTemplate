using IP2Country;
using IP2Country.Datasources;
using IP2Country.WebNet77;
using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// IP轉國家代碼擴充
    /// </summary>
    public static class IP2CountryExtension {
        /// <summary>
        /// 加入IP轉國家代碼轉換器
        /// </summary>
        /// <param name="services">DI服務容器</param>
        /// <returns>DI服務容器</returns>
        public static void AddIP2Country(this IServiceCollection services) {
            services.AddSingleton<IP2CountryResolver>(sp => {
                return new IP2CountryResolver(new IIP2CountryDataSource[] {
                    new WebNet77IPv4CSVFileSource("Resource/IpToCountry.csv"),
                    new WebNet77IPv6CSVFileSource("Resource/IpToCountry.6R.csv")
                });
            });
        }
    }
}
