﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private static int threadsNumber = Environment.ProcessorCount;



        private int blurLength = 0;
        static ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 12 };
        private Boolean masmOn = false;
        private long TimeInTicks = 0;

        public MainWindow()
        {

            InitializeComponent();
            threadsNumber = Environment.ProcessorCount;
            threadValue.Value = threadsNumber;
        }

        BitmapImage bitmapImage;
        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                imgDynamic.Source = new BitmapImage(fileUri);
                bitmapImage = new BitmapImage(fileUri);
            }
        }

        private Bitmap BitmapImagetoBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
            
        }

        public static Bitmap Convolve(Bitmap srcImage, double[,] kernel)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);
            int colorChannels = 3;
            double[] rgb = new double[colorChannels];
            int foff = (kernel.GetLength(0) - 1) / 2;
            int kcenter = 0;
            int kpixel = 0;
            for (int y = foff; y < height - foff; y++)
            {
                for (int x = foff; x < width - foff; x++)
                {
                    for (int c = 0; c < colorChannels; c++)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < colorChannels; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                            }
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap;
            bitmap = BitmapImagetoBitmap(bitmapImage);
            double[,] kernel = ConsoleApp1.Program.GaussianBlur(blurLength, 10, masmOn,ref TimeInTicks, parallelOptions);
            bitmap = Convolve(bitmap, kernel);
            BitmapImage bitmap2 = BitmapToBitmapImage(bitmap);
            text1.Content = TimeInTicks;
            imgDynamic2.Source = bitmap2;
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
            blurLength = (int)slValue.Value;
        }

        private void ThreadSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            threadsNumber = (int)threadValue.Value;
        }

        private void masmOnChange(object sender, RoutedEventArgs e)
        {
            masmOn = masmClick.IsChecked == true;
        }
    }
}

        
