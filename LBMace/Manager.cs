using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBMace
{
    public class Manager
    {
        Data data;
        Solver solver;
        GPGPU gpgpu;
        Postprocess postprocess;
        Form1 form;

        public Manager()
        {
            data = Data.get();
            solver = new Solver();
            gpgpu = new GPGPU();
            postprocess = new Postprocess();
            form = new Form1();
        }

        public void getInput()
        {
            if(data.mapping())
            {

            }
        }
    }
}
