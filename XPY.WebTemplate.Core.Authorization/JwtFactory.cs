using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace XPY.WebTemplate.Core.Authorization {
    public class JwtFactory {
        public JwtBuilder Builder { get; private set; }

        public JwtFactory(JwtBuilder builder) {
            Builder = builder;
        }

        public string BuildToken(string account) {
            return Builder
                .AddClaim(ClaimsIdentity.DefaultRoleClaimType, "Default")
                .AddClaim(ClaimsIdentity.DefaultNameClaimType, account)
                .Build();
        }

        public IDictionary<string, object> ParseToken(string token) {
            var payload = Builder
                .MustVerifySignature()
                .Decode<IDictionary<string, object>>(token);
            return payload;
        }
    }
}
