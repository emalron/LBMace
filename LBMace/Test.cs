using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBMace
{
    public class Test
    {
        private double t;
        public double T
        {
            get
            {
                return t;
            }
            set
            {
                t = value;
            }
        }

        public Test()
        {
            T = 2.0d;
        }
    }
}
