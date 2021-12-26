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

namespace gui
{
    public class ImageClass
    {
        public Image Result;
        public ImageSource source { get; set; }
        public Image result { get; set; }
        public int test { get; set; }
        public Bitmap bmpSource { get; set; }
        public int widthSource { get; set; }
        public int heightSource { get; set; }
        public short[] redSource { get; set; }
        public short[] greenSource { get; set; }
        public short[] blueSource { get; set; }
        public short[] redResult { get; set; }
        public short[] greenResult { get; set; }
        public short[] blueResult { get; set; }

        public int pixels { get; set; }
        public float time { get; set; }
        const int size = 8;
        private BitmapData bmpDataSource;

        //TODO:
        // add c dll import
        // add asm dll import 

        public void setSize()
        {
            heightSource = this.bmpSource.Height;
            widthSource = this.bmpSource.Width;
        }

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
            redSource = new short[pixels];
            greenSource = new short[pixels];
            blueSource = new short[pixels];

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

        // TODO:
        // ADD c function
        // ADD asm function

        //(int heightSource, int widthSource, short[] redSource, short[] blueSource, short[] greenSource)
        public void Filter_c()
        {
            redResult = new short[pixels];
            blueResult = new short[pixels];
            greenResult = new short[pixels];
            //gaussian blur
            short[] kernel = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            int kernel_val = 16;

            //mean blur
            //short[] kernel = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            //int kernel_val = 9;
           

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
        }


        // i guess it read the memory to create R G B result arrays from the previous operations (c or asm)
        //private void assignnewvalues(intptr[] redarray, intptr[] greenarray, intptr[] bluearray)
        //{
        //    //int pixels = bmpbackground.width * bmpbackground.height;
        //    int pixels = bmpsource.width * bmpsource.height;
        //    redresult = new byte[pixels];
        //    greenresult = new byte[pixels];
        //    blueresult = new byte[pixels];


        //    int count = 0;
        //    for (int y = 0; y < pixels / size; y++)
        //    {
        //        for (int x = 0; x < size; x++)
        //        {
        //            //redresult[count] = convert.tobyte((convert.toint32(redbackground[count]) * (100 - alpha) + convert.toint32(redsource[count] )* alpha) / 100);  
        //            //greenresult[count] = convert.tobyte((convert.toint32(greenbackground[count]) * (100 - alpha) + convert.toint32(greensource[count]) * alpha) / 100);
        //            //blueresult[count] = convert.tobyte((convert.toint32(bluebackground[count]) * (100 - alpha) + convert.toint32(bluesource[count++]) * alpha) / 100);
        //            redresult[count] = marshal.readbyte(redarray[y] + 2 * x);
        //            greenresult[count] = marshal.readbyte(greenarray[y] + 2 * x);
        //            blueresult[count++] = marshal.readbyte(bluearray[y] + 2 * x);
        //            // przypisanie do r g b


        //        }
        //    }
        //}

        // makes an image from the separate R G B results
        public Bitmap AfterImageFromRGB()
        {

            Bitmap result_bmp = new Bitmap(bmpSource.Width, bmpSource.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Color c;
            int arrayIndex;

            for (int x = 0; x < bmpSource.Width; x++)
            {
                for (int y = 0; y < bmpSource.Height; y++)
                {
                    arrayIndex = y * bmpSource.Width + x; // obliczenie indeksu
                    c = System.Drawing.Color.FromArgb(255, redResult[arrayIndex], greenResult[arrayIndex], blueResult[arrayIndex]); // znajdywanie koloru na podstawie wartości rgb
                    //c = System.Drawing.Color.FromArgb(255,128, 255, 6); // znajdywanie koloru na podstawie wartości rgb
                    result_bmp.SetPixel(x, y, c); // ustawianie piksela
                    //var handle = result_bmp.GetHbitmap();
                }
            }
            return result_bmp;
        }

        // makes an image from the bitmap generated by AfterImageFromRGB
        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
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

