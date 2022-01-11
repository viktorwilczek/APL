using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using filterC;
using System.Threading.Tasks;
using System.Diagnostics;

namespace gui
{
    public class ImageClass
    {
        //final image after filtering
        public Image Result;
        //the source image
        public ImageSource source { get; set; }
        //the source image in bitmap format
        public Bitmap bmpSource { get; set; }
        //width of the image
        public int widthSource { get; set; }
        //height of the image
        public int heightSource { get; set; }
        //red channel of source image
        public short[] redSource { get; set; }
        //green channel of source image
        public short[] greenSource { get; set; }
        //blue channel of source image
        public short[] blueSource { get; set; }
        //red channel of result image
        public short[] redResult { get; set; }
        //red channel of result image
        public short[] greenResult { get; set; }
        //red channel of result image
        public short[] blueResult { get; set; }
        //number of pixels in the image
        public int pixels { get; set; }
        //bitmapdata of image
        private BitmapData bmpDataSource;

        //import asm dll
        [System.Runtime.InteropServices.DllImport("kernel_filter_asm.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void _kernel_filter(int height, int width, IntPtr source, IntPtr result_rgb, IntPtr kernel, int kernel_val);


        //set size of the image
        public void setSize()
        {
            heightSource = this.bmpSource.Height;
            widthSource = this.bmpSource.Width;
        }


        //creates separate RGB channels
        public void createRGB_source()
        {
            //BitmapData Specifies the attributes of a bitmap image
            BitmapData bmpDataImg;
            Rectangle rect = new Rectangle(0, 0, bmpSource.Width, bmpSource.Height);
            //LockBits Locks a Bitmap into system memory.
            bmpDataImg = bmpSource.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //Gets pointer to the first pixel in the bitmap.  
            IntPtr ptr = bmpDataImg.Scan0;
            //Gets the number of necessary space in the bytes array.
            pixels = bmpDataImg.Width * bmpSource.Height;
            int bytes = bmpDataImg.Stride * bmpSource.Height;
            byte[] rgbValues = new byte[bytes];
            //RGB arrays
            redSource = new short[pixels];
            greenSource = new short[pixels];
            blueSource = new short[pixels];

            //from bitmap to rgb
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            int count = 0;
            int stride = bmpDataImg.Stride;

            //loops to create R G B arrays based on bitmap
            for (int row = 0; row < bmpDataImg.Height; row++)
            {
                for (int column = 0; column < bmpDataImg.Width; column++)
                {
                    blueSource[count] = (short)(rgbValues[(column * 3) + row * stride]);
                    greenSource[count] = (short)(rgbValues[(column * 3 + 1) + row * stride]);
                    redSource[count++] = (short)(rgbValues[(column * 3 + 2) + row * stride]);
                }
            }
            bmpDataSource = bmpDataImg;
            bmpSource.UnlockBits(bmpDataImg);
        }


        //calls the filter implemented in assembly
        public string filter_asm(string filter_type)
        {
            //prepare result array
            redResult = new short[pixels];
            blueResult = new short[pixels];
            greenResult = new short[pixels];

            //defining kernel filter base on user choice
            //gaussian blur
            short[] kernel = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            int kernel_val = 16;

            //sharpen kernel filter
            if (filter_type.Equals("Sharpen"))
            {
                kernel[0] = 0;
                kernel[1] = -1;
                kernel[2] = 0;
                kernel[3] = -1;
                kernel[4] = 5;
                kernel[5] = -1;
                kernel[6] = 0;
                kernel[7] = -1;
                kernel[8] = 0;
                kernel_val = 1;
            }

            //edge detection kernel filter
            if (filter_type.Equals("Edge detection"))
            {
                kernel[0] = -1;
                kernel[1] = -1;
                kernel[2] = -1;
                kernel[3] = -1;
                kernel[4] = 8;
                kernel[5] = -1;
                kernel[6] = -1;
                kernel[7] = -1;
                kernel[8] = -1;
                kernel_val = 1;
            }

            //mean blur kernel filter
            if (filter_type.Equals("Mean blur"))
            {
                kernel[0] = 1;
                kernel[1] = 1;
                kernel[2] = 1;
                kernel[3] = 1;
                kernel[4] = 1;
                kernel[5] = 1;
                kernel[6] = 1;
                kernel[7] = 1;
                kernel[8] = 1;
                kernel_val = 9;
            }

            //allocating memomry for source and result rgb channels
            IntPtr kernelPtr = Marshal.AllocHGlobal(18);
            Marshal.Copy(kernel, 0, kernelPtr, 9);

            IntPtr redSourcePtr = Marshal.AllocHGlobal(pixels * 2);
            Marshal.Copy(redSource, 0, redSourcePtr, pixels);
            IntPtr redResultPtr = Marshal.AllocHGlobal(pixels * 2);

            IntPtr blueSourcePtr = Marshal.AllocHGlobal(pixels * 2);
            Marshal.Copy(blueSource, 0, blueSourcePtr, pixels);
            IntPtr blueResultPtr = Marshal.AllocHGlobal(pixels * 2);

            IntPtr greenSourcePtr = Marshal.AllocHGlobal(pixels * 2);
            Marshal.Copy(greenSource, 0, greenSourcePtr, pixels);
            IntPtr greenResultPtr = Marshal.AllocHGlobal(pixels * 2);

            //starting the stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            //invoke the asm kernel filter in parallel respectively on R,G and B channels
            Parallel.Invoke(() =>
                            {
                                _kernel_filter(heightSource, widthSource, redSourcePtr, redResultPtr, kernelPtr, kernel_val);
                            },
                            () =>
                            {
                                _kernel_filter(heightSource, widthSource, blueSourcePtr, blueResultPtr, kernelPtr, kernel_val);
                            },
                            () =>
                            {
                                _kernel_filter(heightSource, widthSource, greenSourcePtr, greenResultPtr, kernelPtr, kernel_val);
                            }
                           );

            //stop the stopwatch and format return string
            stopwatch.Stop();
            TimeSpan tspan = stopwatch.Elapsed;
            String elapsedTime = String.Format(" {0:00}:{1:00}.{2:000} ",
                 tspan.Minutes, tspan.Seconds, tspan.Milliseconds);

            //used to copy values from pointer to short array
            short[] redResultTmp = new short[pixels];
            short[] blueResultTmp = new short[pixels];
            short[] greenResultTmp = new short[pixels];

            //copy the result values
            Marshal.Copy(redResultPtr, redResultTmp, 0, pixels);
            Marshal.Copy(blueResultPtr, blueResultTmp, 0, pixels);
            Marshal.Copy(greenResultPtr, greenResultTmp, 0, pixels);

            //copy to final destination
            for (int i = 0; i < pixels; i++)
            {
                redResult[i] = redResultTmp[i];

                if (redResultTmp[i] < 0)
                {
                    redResult[i] = 0;
                }
                if (redResultTmp[i] > 255)
                {
                    redResult[i] = 255;
                }

            }

            for (int i = 0; i < pixels; i++)
            {
                greenResult[i] = greenResultTmp[i];
                if (greenResultTmp[i] < 0)
                {
                    greenResult[i] = 0;
                }
                if (greenResultTmp[i] > 255)
                {
                    greenResult[i] = 255;
                }

            }

            for (int i = 0; i < pixels; i++)
            {
                blueResult[i] = blueResultTmp[i];
                if (blueResultTmp[i] < 0)
                {
                    blueResult[i] = 0;
                }
                if (blueResultTmp[i] > 255)
                {
                    blueResult[i] = 255;
                }

            }

            return elapsedTime;
        }

        //calls the filter implemented in c#
        public String Filter_c(String filter_type)
        {
            //prepare result array
            redResult = new short[pixels];
            blueResult = new short[pixels];
            greenResult = new short[pixels];

            //defining kernel filter base on user choice
            //gaussian blur
            short[] kernel = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            int kernel_val = 16;

            //sharpen kernel filter
            if (filter_type.Equals("Sharpen"))
            {
                kernel[0] = 0;
                kernel[1] = -1;
                kernel[2] = 0;
                kernel[3] = -1;
                kernel[4] = 5;
                kernel[5] = -1;
                kernel[6] = 0;
                kernel[7] = -1;
                kernel[8] = 0;
                kernel_val = 1;
            }

            //edge detection kernel filter
            if (filter_type.Equals("Edge detection"))
            {
                kernel[0] = -1;
                kernel[1] = -1;
                kernel[2] = -1;
                kernel[3] = -1;
                kernel[4] = 8;
                kernel[5] = -1;
                kernel[6] = -1;
                kernel[7] = -1;
                kernel[8] = -1;
                kernel_val = 1;
            }

            //mean blur kernel filter
            if (filter_type.Equals("Mean blur"))
            {
                kernel[0] = 1;
                kernel[1] = 1;
                kernel[2] = 1;
                kernel[3] = 1;
                kernel[4] = 1;
                kernel[5] = 1;
                kernel[6] = 1;
                kernel[7] = 1;
                kernel[8] = 1;
                kernel_val = 9;
            }

            //start stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();

            //invoke the c# kernel filter in parallel respectively on R,G and B channels
            Filter Filter = new Filter();
            Parallel.Invoke(() =>
                            {
                                Filter.kernel_filter(heightSource, widthSource, redSource, redResult, kernel, kernel_val);
                            },
                            () =>
                            {
                                Filter.kernel_filter(heightSource, widthSource, blueSource, blueResult, kernel, kernel_val);
                            },
                            () =>
                            {
                                Filter.kernel_filter(heightSource, widthSource, greenSource, greenResult, kernel, kernel_val);
                            }
                           );

            //stop the stopwatch and format return string
            stopwatch.Stop();
            TimeSpan tspan = stopwatch.Elapsed;
            String elapsedTime = String.Format(" {0:00}:{1:00}.{2:000} ",
                 tspan.Minutes, tspan.Seconds, tspan.Milliseconds );

            return elapsedTime;
        }

        // makes an image from the separate R G B results
        public Bitmap AfterImageFromRGB()
        {
            //prepare result bitmap
            Bitmap result_bmp = new Bitmap(bmpSource.Width, bmpSource.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Color c;

            //loop to create the result bitmap
            int arrayIndex;
            for (int x = 0; x < bmpSource.Width; x++)
            {
                for (int y = 0; y < bmpSource.Height; y++)
                {
                    //calculate index
                    arrayIndex = y * bmpSource.Width + x; 
                    //set color based on rgb values
                    c = System.Drawing.Color.FromArgb(255, redResult[arrayIndex], greenResult[arrayIndex], blueResult[arrayIndex]); 
                    //set pixel based on location and color
                    result_bmp.SetPixel(x, y, c);
                }
            }
            return result_bmp;
        }

        // makes an image from the bitmap generated by AfterImageFromRGB
        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            //creates image to be set in the output window
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

    

    }
}

