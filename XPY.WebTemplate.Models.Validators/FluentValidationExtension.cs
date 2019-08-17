using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XPY.WebTemplate.Models.Validators {
    public static class FluentValidationExtension {
        public static void AddFluentValidation(this IServiceCollection services) {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();

            var genType = typeof(IValidator<>);

            foreach (var type in allTypes) {
                var validatorType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == genType);
                if (validatorType == null) continue;

                var servicesType = genType.MakeGenericType(validatorType.GetGenericArguments());
                services.AddTransient(servicesType, type);
            }
        }
    }
}
