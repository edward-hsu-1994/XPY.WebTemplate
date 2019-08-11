using IP2Country;
using IP2Country.Datasources;
using IP2Country.WebNet77;
using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection {
    public static class IP2CountryExtension {
        public static IServiceCollection AddIP2Country(this IServiceCollection services) {
            return services.AddSingleton<IP2CountryResolver>(sp => {
                return new IP2CountryResolver(new IIP2CountryDataSource[] {
                    new WebNet77IPv4CSVFileSource("Resource/IpToCountry.csv"),
                    new WebNet77IPv6CSVFileSource("Resource/IpToCountry.6R.csv")
                });
            });
        }
    }
}
