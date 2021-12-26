﻿using gui;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace APL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageClass img;
        public MainWindow()
        {
            img = new ImageClass();
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Uri sourceUri = null;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Images only. | *.jpg; *.jpeg; *.png; *.bmp; *.rgb";

                if (openFileDialog.ShowDialog() == true)
                {
                    sourceUri = new Uri(openFileDialog.FileName);
                }
                if (sourceUri != null)
                {
                    img.source = Source.Source;
                    FileInfo file = new FileInfo(openFileDialog.FileName);
                    Bitmap bmpImp = new Bitmap(openFileDialog.FileName);
                    ///Validator 
                    Source.Source = new BitmapImage(sourceUri);
                    img.bmpSource = bmpImp;
                    img.setSize();
                    var validator = new ImageClassValidator();
                   validator.Validate(img);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception)
            {
                MessageBox.Show("There is an error");
            }

        }
        private void RunC_Click(object sender, RoutedEventArgs e) {
            img.createRGB_source();
            img.Filter_c();
            var result_img = img.BitmapToImageSource(img.AfterImageFromRGB());
            Result.Source = result_img;
        }

    }
}
