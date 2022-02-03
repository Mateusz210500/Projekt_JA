using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Security;

namespace ConsoleApp1
{
    public class MasmConnector
    {
        
        [DllImport(@"C:\Users\mateu\source\repos\Studies\Projekt_JA\test_ja\x64\Debug\asembler.dll")]
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static extern int Multiply(double[] a, double b, double[] c);

        
        [DllImport(@"C:\Users\mateu\source\repos\Studies\Projekt_JA\test_ja\x64\Debug\asembler2.dll")]
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static extern int Blur(int a, int b, int c);
    }
}
