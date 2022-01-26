using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WpfApp1
{
    public class MasmConnector
    {
        [DllImport("../../../x64/Debug/asembler.dll")]
        public static extern int Adding(double[] a, double b, double[] c);
    }
}
