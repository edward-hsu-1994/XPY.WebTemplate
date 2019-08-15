using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Services.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class InjectAttribute : Attribute {
        public ServiceLifetime LifeTime { get; private set; }
        public Type ServiceType { get; set; }
        public InjectAttribute(ServiceLifetime lifetime) {
            LifeTime = lifetime;
        }
    }
}
