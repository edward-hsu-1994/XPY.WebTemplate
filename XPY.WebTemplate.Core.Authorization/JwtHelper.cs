using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace XPY.WebTemplate.Core.Authorization {
    public class JwtHelper {
        public JwtBuilder Builder { get; private set; }

        public JwtHelper(JwtBuilder builder) {
            Builder = builder;
        }

        public string BuildToken(string account) {
            return JwtBearerDefaults.AuthenticationScheme + " " + Builder
                .AddClaim(ClaimsIdentity.DefaultRoleClaimType, "Default")
                .AddClaim(ClaimsIdentity.DefaultNameClaimType, account)
                .Build();
        }

        public IDictionary<string, object> ParseToken(string token) {
            token = token?.Trim()?.Substring(JwtBearerDefaults.AuthenticationScheme.Length + 1);
            var payload = Builder
                .MustVerifySignature()
                .Decode<IDictionary<string, object>>(token);
            return payload;
        }
    }
}
