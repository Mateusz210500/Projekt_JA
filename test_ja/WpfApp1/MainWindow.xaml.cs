using System;
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
        public MainWindow()
        {
            InitializeComponent();
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

        private static double[] MatrixToArray(double[,] prop)
        {
            double[] result = new double[prop.Length];
            int index = 0;

            for (var i = 0; i <= prop.GetUpperBound(0); i++)
            {
                for (var j = 0; j <= prop.GetUpperBound(1); j++)
                    result[index++] = prop[i, j];
            }

            return result;
        }
        private static double[,] ArrayToMatrix(double[] flat, int m, int n)
        {
            if (flat.Length != m * n)
            {
                throw new ArgumentException("Invalid length");
            }
            double[,] ret = new double[m, n];
            // BlockCopy uses byte lengths: a double is 8 bytes
            Buffer.BlockCopy(flat, 0, ret, 0, flat.Length * sizeof(double));
            return ret;
        }

        private static double[] ConnectArrays(double[] first, double[] second)
        {
            double[] arr3 = new double[first.Length + second.Length];
            Array.Copy(first, arr3, first.Length);
            Array.Copy(second, 0, arr3, first.Length, second.Length);
            return arr3;
        }

        //private static Bitmap Blur(Bitmap image, int blurSize)
        //{
        //    Bitmap blurred = new Bitmap(image.Width, image.Height);
        //    for (int x = blurSize; x < image.Width - blurSize; x++)
        //    {
        //        for(int y = blurSize; y < image.Height - blurSize; y++)
        //        {
        //            try
        //            {
        //                System.Drawing.Color prevX = image.GetPixel(x - blurSize, y);
        //                System.Drawing.Color nextX = image.GetPixel(x + 1, y);
        //                System.Drawing.Color prevy = image.GetPixel(x, y - 1);
        //                System.Drawing.Color nextY = image.GetPixel(x, y + 1);

        //                int avgR = (int)((prevX.R + nextX.R + prevy.R + nextY.R) / 4);
        //                int avgG = (int)((prevX.G + nextX.G + prevy.G + nextY.G) / 4);
        //                int avgB = (int)((prevX.B + nextX.B + prevy.B + nextY.B) / 4);

        //                blurred.SetPixel(x, y, System.Drawing.Color.FromArgb(avgR, avgG, avgB));
        //            }
        //            catch (Exception) { }
        //        }
        //    }
        //    return blurred;
        //}

        private static int Adding(double[] a, double b, double[] c)
        {
            return MasmConnector.Adding(a, b, c);
        }

        public static double[,] GaussianBlur(int length, int weight)
        {
            double[,] kernel = new double[length, length];
            double kernelSum = 0;
            int foff = (length - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * weight * weight);
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                }
            }
            //for (int y = 0; y < length; y++)
            //{
            //    for (int x = 0; x < length; x++)
            //    {
            //        kernel[y, x] = kernel[y, x] * 1d / kernelSum;
            //    }
            //}

            //double[] kernel2 = MatrixToArray(kernel);
            //double[] kernel3 = new double[16];
            //double[] temp;
            //double B = 1d / kernelSum;
            //for (int i = 0; i < 3; i++)
            //{
            //    temp = kernel2.Skip(i * 4).Take(16).ToArray();
            //    Adding(temp, B, kernel3);
            //    kernel3 = ConnectArrays(kernel3, temp);
            //}
            //Adding(kernel2, B, kernel3);

            double[] kernel2 = MatrixToArray(kernel);
            double[] kernel3 = new double[0];
            double[] kernel5 = new double[16];
            double B = 1d / kernelSum;
            for (int i = 0; i < 4; i++)
            {
                double[] temp = kernel2.Skip(i * 16).Take(16).ToArray();
                Adding(temp, B, kernel5);
                kernel3 = ConnectArrays(kernel3, kernel5);
            }

            double[,] kernel4 = ArrayToMatrix(kernel3, length, length);


            return kernel4;
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
            double[,] kernel = GaussianBlur(8, 10);
            bitmap = Convolve(bitmap, kernel);
            BitmapImage bitmap2 = BitmapToBitmapImage(bitmap);
            imgDynamic2.Source = bitmap2;
        }

        private void BtnMASM_Click(object sender, RoutedEventArgs e)
        {

            //double[] a = new double[16] { 0, 7, 2, 3, 0, 1, 2, 3, 0, 1, 2, 8, 0, 1, 2, 3 };
            //double[] b = new double[16];
            //MasmConnector.Adding(a, 5.0, b);
            //text1.Content = b.Sum();

            //double[] kernel2 = new double[64] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            double[] kernel2 = new double[64] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
            double[] kernel3 = new double[0];
            double[] kernel4 = new double[16];
            double B = 4;
            for (int i = 0; i < 4; i++)
            {
                double[] temp = kernel2.Skip(i * 16).Take(16).ToArray();
                Adding(temp, B, kernel4);
                kernel3 = ConnectArrays(kernel3, kernel4);
            }


            text1.Content = 5;
        }
    }

}

        
