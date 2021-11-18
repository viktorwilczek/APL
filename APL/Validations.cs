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
            List<int> weighList = new List<int>() { 1366, 1920, 3840 };
            List<int> heightsList = new List<int>() { 768, 1080, 2160 };

            RuleFor(x => x.heightSource)
                .NotEmpty()
                .Equal(x => x.heightBacground)
                .OnAnyFailure(x =>
                {
                    throw new ArgumentException($"Parameter {nameof(x.heightSource)} is invalid.");
                });
            RuleFor(x => x.widthSource)
              .NotEmpty()
              .Equal(x => x.widthBackground)
              .OnAnyFailure(x =>
              {
                  throw new ArgumentException($"Parameter {nameof(x.widthSource)} is invalid.");
              });
        }
    }
}
