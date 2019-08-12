using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// 客戶端裝置追蹤擴充
    /// </summary>
    public static class DeviceDetectorExtension {
        /// <summary>
        /// 加入客戶端裝置追蹤擴充
        /// </summary>
        /// <param name="services">DI服務容器</param>
        /// <returns>DI服務容器</returns>
        public static IServiceCollection AddDeviceDetector(this IServiceCollection services) {
            return services.AddScoped<DeviceDetector>(sp => {
                var httpContext = sp.GetService<IHttpContextAccessor>().HttpContext;

                DeviceDetector detector = null;
                if (httpContext.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent)) {
                    detector = new DeviceDetector(userAgent);
                    detector.SetCache(new DictionaryCache());
                    detector.Parse();
                }

                return detector;
            });
        }
    }
}
