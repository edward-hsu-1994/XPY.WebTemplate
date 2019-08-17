using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XPY.WebTemplate.Models.Validators {
    public class SampleModelValidator : AbstractValidator<SampleModel> {
        public SampleModelValidator() {
            RuleFor(x => x.Id).NotNull().MinimumLength(5).MaximumLength(32).WithMessage("帳號長度需再5至32個字元內");
            RuleFor(x => x.Password).NotNull().MinimumLength(10).WithMessage("密碼至少要有10個字元");
        }
    }
}
