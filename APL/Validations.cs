using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace gui
{
    public class ImageClassValidator : AbstractValidator<ImageClass>
    {
        public ImageClassValidator()
        {
            List<int> widthList = new List<int>() { 300, 1366, 1920, 1800, 3840 };
            List<int> heightsList = new List<int>() { 300, 768, 1080, 1800, 2160 };

            RuleFor(x => x.heightSource)
                .NotEmpty()
                .GreaterThan(0)
                .Must(x => heightsList.Contains(x))
                   .OnAnyFailure(x =>
                   {
                       throw new ArgumentException($"Parameter {nameof(x.heightSource)} is invalid.");
                   });

            RuleFor(x => x.widthSource)
               .NotEmpty()
               .GreaterThan(0)
               .Must(x => widthList.Contains(x))
                  .OnAnyFailure(x =>
                  {
                      throw new ArgumentException($"Parameter {nameof(x.widthSource)} is invalid.");
                  });
        }
    }
}
