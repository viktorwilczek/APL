using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace gui
{
    public class ImageClassValidator : AbstractValidator<ImageClass>
    {
        [Obsolete]
        public ImageClassValidator()
        {
            //possible values
            List<int> widthList = new List<int>() { 200, 300, 600, 1800, 3000, 5000 };
            List<int> heightsList = new List<int>() { 200, 300, 600, 1800, 3000, 5000};

            //check the height
            RuleFor(x => x.heightSource)
                .NotEmpty()
                .GreaterThan(0)
                .Must(x => heightsList.Contains(x))
                   .OnAnyFailure(x =>
                   {
                       throw new ArgumentException($"Parameter {nameof(x.heightSource)} is invalid. Allowed sizes are: 200x200, 300x300, 600x600, 1800x1800, 3000x3000");
                   });

            //check the width
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
