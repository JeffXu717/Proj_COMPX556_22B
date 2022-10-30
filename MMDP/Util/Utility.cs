using System;
using System.Collections.Generic;
using System.Text;

namespace MMDP.Util
{
    public static class Utility
    {
        public static int Mod(this int a, int b)
        {
            int c = (int) Math.Floor((double)a / b);
            return a - c * b;
        }
    }
}
