using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.Mvc {
    public class FormJsonModelBinderProvider : IModelBinderProvider {
        public IModelBinder GetBinder(ModelBinderProviderContext context) {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.BindingInfo?.BindingSource?.DisplayName == "FormJson") {
                return new FormJsonModelBinder();
            }

            return null;
        }
    }
}
