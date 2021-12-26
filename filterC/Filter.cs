using System;

namespace filterC
{
    public class Filter
    {
        public void kernel_filter(int height, int width, short[] source, short[] result_rgb, short[] kernel, int kernel_value)
        {
            int pixels = height * width;
            int width_counter = 0;
            for (int i = 0; i < pixels; i++)
            {
                // left border of image
                if ((i % width) == 0)
                {
                    // set black color
                    result_rgb[i] = 0;
                    width_counter += width;
                }

                // top border of image
                else if (i < width)
                {
                    // set black color
                    result_rgb[i] = 0;
                }

                //right border of image
                else if (i == width_counter - 1)
                {
                    // set black color
                    result_rgb[i] = 0;
                }

                //bottom border of image
                else if (i > pixels - width)
                {
                    // set black color
                    result_rgb[i] = 0;
                }

                // all the rest
                else
                {
                    result_rgb[i] = ((short)(((source[i + (-width - 1)] * kernel[0]) +
                    source[i + (-width - 1)] * kernel[1] +
                    source[i + (-width + 1)] * kernel[2] +
                    source[(i - 1)] * kernel[3] +
                    source[(i)] * kernel[4] +
                    source[(i + 1)] * kernel[5] +
                    source[(i + (width - 1))] * kernel[6] +
                    source[(i + (width))] * kernel[7] +
                    source[(i + (width + 1))] * kernel[8]) / kernel_value));

                    if (result_rgb[i] < 0)
                    {
                        result_rgb[i] = 0;
                    }
                    if (result_rgb[i] > 255)
                    {
                        result_rgb[i] = 255;
                    }
                }
            }
        }
    }
}

