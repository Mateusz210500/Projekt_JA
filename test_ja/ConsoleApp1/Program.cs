using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {

        public static double[] MatrixToArray(double[,] prop)
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

        public static double[,] ArrayToMatrix(double[] flat, int m, int n)
        {
            if (flat.Length != m * n)
            {
                throw new ArgumentException("Invalid length");
            }
            double[,] ret = new double[m, n];
            Buffer.BlockCopy(flat, 0, ret, 0, flat.Length * sizeof(double));
            return ret;
        }

        public static double[] ConnectArrays(double[] first, double[] second)
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

        public static double[,] GaussianBlur(int length, int weight, Boolean masm, ref long resultTime, ParallelOptions parallelOptions)
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
            double[] kernel2 = ConsoleApp1.Program.MatrixToArray(kernel);
            double[] kernel3 = new double[0];
            double[] kernel5 = new double[16];
            double B = 1d / kernelSum;
            int forIter = length * length / 16;
            if (!masm)
            {
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
            }
            else
            {
                timer.Start();
                for (int i = 0; i < length * length / 16; i++)
                {
                    double[] temp = kernel2.Skip(i * 16).Take(16).ToArray();
                    Multiply(temp, B, kernel5);
                    kernel3 = ConsoleApp1.Program.ConnectArrays(kernel3, kernel5);
                }
                timer.Stop();
                resultTime = timer.ElapsedTicks;
                double[,] kernel4 = ConsoleApp1.Program.ArrayToMatrix(kernel3, length, length);
                Console.WriteLine("masm");
                return kernel4;
            }
        }
    }
}
