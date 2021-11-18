using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace gui
{
    class ImageClass
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
        public byte[] redResult { get; set; }
        public byte[] greenResult { get; set; }
        public byte[] blueResult { get; set; }

        int pixels { get; set; }
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
            BitmapData bmpDataImg;
            Rectangle rect = new Rectangle(0, 0, bmpSource.Width, bmpSource.Height);
            bmpDataImg = bmpSource.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //Gets pointer to the first pixel in the bitmap.  
            IntPtr ptr = bmpDataImg.Scan0;
            //Gets the number of necessary space in the bytes's array.
            int pixels = bmpDataImg.Width * bmpSource.Height;
            int bytes = bmpDataImg.Stride * bmpSource.Height;
            byte[] rgbValues = new byte[bytes];
            redSource = new short[pixels];
            greenSource = new short[pixels];
            blueSource = new short[pixels];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            int count = 0;
            int stride = bmpDataImg.Stride;


            //loops to create R G B tables based on bitmap
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

        // i guess it read the memory to create R G B result arrays from the previous operations (c or asm)
        private void AssignNewValues(IntPtr[] redArray, IntPtr[] greenArray, IntPtr[] blueArray)
        {
            int pixels = bmpBackground.Width * bmpBackground.Height;
            redResult = new byte[pixels];
            greenResult = new byte[pixels];
            blueResult = new byte[pixels];


            int count = 0;
            for (int y = 0; y < pixels / size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    //redResult[count] = Convert.ToByte((Convert.ToInt32(redBackground[count]) * (100 - alpha) + Convert.ToInt32(redSource[count] )* alpha) / 100);  
                    //greenResult[count] = Convert.ToByte((Convert.ToInt32(greenBackground[count]) * (100 - alpha) + Convert.ToInt32(greenSource[count]) * alpha) / 100);
                    //blueResult[count] = Convert.ToByte((Convert.ToInt32(blueBackground[count]) * (100 - alpha) + Convert.ToInt32(blueSource[count++]) * alpha) / 100);
                    redResult[count] = Marshal.ReadByte(redArray[y] + 2 * x);
                    greenResult[count] = Marshal.ReadByte(greenArray[y] + 2 * x);
                    blueResult[count++] = Marshal.ReadByte(blueArray[y] + 2 * x);
                    // przypisanie do r g b


                }
            }
        }

        // makes an image from the separate R G B results
        public Bitmap AfterImageFromRGB()
        {

            Bitmap result_bmp = new Bitmap(bmpBackground.Width, bmpBackground.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Color c;
            int arrayIndex;

            for (int x = 0; x < bmpBackground.Width; x++)
            {
                for (int y = 0; y < bmpBackground.Height; y++)
                {
                    arrayIndex = y * bmpBackground.Width + x; // obliczenie indeksu
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
