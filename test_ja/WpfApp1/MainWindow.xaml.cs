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

        private static int Multiply(double[] a, double b, double[] c)
        {
            return MasmConnector.Multiply(a, b, c);
        }

        private static int Blur(int a, int b, int c)
        {
            return MasmConnector.Blur(a, b, c);
        }

        public static double[,] GaussianBlur(int length, int weight, Boolean masm, ref long resultTime)
        {
            var timer = new Stopwatch();
            length = length * 4;
            double[,] kernel = new double[length, length];
            double kernelSum = 0;
            int foff = (length - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {

                Parallel.For(-foff, foff + 1, parallelOptions, x =>
                {
                    //    for (int x = -foff; x <= foff; x++)
                    //{
                    if (masm)
                    {
                        timer.Start();
                        int temp = Blur(y, x, weight);
                        distance = (double)temp / 100;
                        timer.Stop();
                    }
                    else
                    {
                        timer.Start();
                        int temp = ((y * y) + (x * x)) * 100 / (2 * weight * weight);
                        distance = (double)temp / 100;
                        timer.Stop();
                    }
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                });
            }
            double[] kernel2 = MatrixToArray(kernel);
            double[] kernel3 = new double[0];
            double[] kernel5 = new double[16];
            double B = 1d / kernelSum;
            int forIter = length * length / 16;
            if (!masm) {
                    timer.Start();
                    for (int y = 0; y < length; y++)
                    {
                        for (int x = 0; x < length; x++)
                        {
                            kernel[y, x] = kernel[y, x] * B;
                        }
                }
                timer.Stop();
                resultTime = timer.ElapsedTicks;
                Console.WriteLine("c#");
                return kernel;
            }else {
                    timer.Start();
                for (int i = 0; i < length*length/16; i++)
                {
                    double[] temp = kernel2.Skip(i * 16).Take(16).ToArray();
                        Multiply(temp, B, kernel5);
                        kernel3 = ConnectArrays(kernel3, kernel5);
            }
            timer.Stop();
                resultTime = timer.ElapsedTicks;
                double[,] kernel4 = ArrayToMatrix(kernel3, length, length);
                Console.WriteLine("masm");
                return kernel4;
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
            double[,] kernel = GaussianBlur(blurLength, 10, masmOn,ref TimeInTicks);
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

        
