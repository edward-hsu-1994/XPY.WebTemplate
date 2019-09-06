using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.Mvc {
    public class FromFormJsonAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider {
        public BindingSource BindingSource => new BindingSource("Form", "FormJson", true, true);

        public string Name { get; set; }
    }
}
