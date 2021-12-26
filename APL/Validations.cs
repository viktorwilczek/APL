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
            List<int> widthList = new List<int>() { 200, 300, 600, 1920, 1800, 3000 };
            List<int> heightsList = new List<int>() { 200, 300, 600, 1080, 1800, 3000};

            RuleFor(x => x.heightSource)
                .NotEmpty()
                .GreaterThan(0)
                .Must(x => heightsList.Contains(x))
                   .OnAnyFailure(x =>
                   {
                       throw new ArgumentException($"Parameter {nameof(x.heightSource)} is invalid. Allowed sizes are: 200x200, 300x300, 600x600, 1800x1800, 3000x3000");
                   });

            RuleFor(x => x.widthSource)
               .NotEmpty()
               .GreaterThan(0)
               .Must(x => widthList.Contains(x))
                  .OnAnyFailure(x =>
                  {
                      throw new ArgumentException($"Parameter {nameof(x.widthSource)} is invalid. Allowed sizes are: 200x200, 300x300, 600x600, 1800x1800, 3000x3000");
                  });
        }
    }
}
