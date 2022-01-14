using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WpfApp1
{
    class Class1
    {
        [DllImport("../../../Debug/asembler2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int adding(int a, int b, int c);
    }
}
