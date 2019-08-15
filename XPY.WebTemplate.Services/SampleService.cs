using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using XPY.WebTemplate.Core.Authorization;
using XPY.WebTemplate.Services.Attributes;

namespace XPY.WebTemplate.Services {
    [Inject(ServiceLifetime.Scoped)]
    public class SampleService {
        public JwtHelper JwtHelper { get; private set; }
        public SampleService(JwtHelper jwt) {
            JwtHelper = jwt;
        }
    }
}
