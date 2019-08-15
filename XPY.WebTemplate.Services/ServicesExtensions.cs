using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using XPY.WebTemplate.Services.Attributes;

namespace Microsoft.Extensions.DependencyInjection {
    public static class ServicesExtensions {
        public static void AddServices(this IServiceCollection services) {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in allTypes) {
                var attr = type.GetCustomAttribute<InjectAttribute>();
                if (attr == null) continue;

                services.Add(new ServiceDescriptor(attr.ServiceType ?? type, type, attr.LifeTime));
            }
        }
    }
}
