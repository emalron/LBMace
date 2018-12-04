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
        States myState;

        enum States
        {
            IDLE,
            READY
        }

        public Manager()
        {
            data = Data.get();
            solver = new Solver();
            gpgpu = new GPGPU();
            postprocess = new Postprocess();
        }

        public void setGeometry()
        {
            if(data.mapping())
            {

            }
        }
    }
}
