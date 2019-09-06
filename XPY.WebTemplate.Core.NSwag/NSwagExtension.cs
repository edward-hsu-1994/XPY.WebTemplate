using Microsoft.AspNetCore.Builder;
using NSwag;
using NSwag.Generation.Processors.Security;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// NSwag擴充
    /// </summary>
    public static class NSwagExtension {
        /// <summary>
        /// 加入NSwag
        /// </summary>
        /// <param name="services">DI服務容器</param>
        /// <returns>DI服務容器</returns>
        public static void AddNSwag(
            this IServiceCollection services,
            string title,
            string description = null) {
            services.AddSwaggerDocument(config => {
                config.Title = title;
                config.Description = description;
                config.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();

                // ref: https://github.com/RSuter/NSwag/issues/869
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("apiKey"));
                /*config.OperationProcessors.Add(new AuthorizeOperationProcessor());
                config.OperationProcessors.Add(new OptionParamProcessor());
                config.OperationProcessors.Add(new StringEnumParamProcessor());
                config.OperationProcessors.Add(new FixFormFileParamProcessor());
                config.OperationProcessors.Add(new DefaultEnumParamProcessor());
                config.OperationProcessors.Add(new ConsumesAttributeProcessor());*/
                config.DocumentProcessors.Add(new SecurityDefinitionAppender(
                    "apiKey",
                    new string[0],
                    new NSwag.OpenApiSecurityScheme() {
                        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Description = "JWT(Bearer) 存取權杖"
                    }));
            });
        }

        /// <summary>
        /// 使用OpenApi產生並使用SwaggerUi3
        /// </summary>
        /// <param name="app">應用程式建構器</param>
        /// <returns>應用程式建構器</returns>
        public static IApplicationBuilder UseOpenApiAndSwaggerUi3(this IApplicationBuilder app) {
            app.UseOpenApi(configure => {
                configure.PostProcess = (doc, req) => {
                    doc.Schemes.Clear();
                    doc.Schemes.Add(Enum.Parse<OpenApiSchema>(req.Scheme, true));
                };
            });
            return app.UseSwaggerUi3();
        }
    }
}
