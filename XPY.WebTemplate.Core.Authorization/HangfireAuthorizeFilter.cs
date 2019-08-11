using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.Authorization {
    public class HangfireAuthorizeFilter : IDashboardAuthorizationFilter {
        public bool Authorize(DashboardContext context) {
            return true; // 任何人都可以直接觀看
        }
    }
}
