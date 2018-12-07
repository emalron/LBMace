using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LBMace
{
    public class Manager
    {
        Data data;
        Solver solver;
        GPGPU gpu;
        Postprocess postprocess;
        public delegate void chartCallback(double a, double b);
        public chartCallback cCb;
        public States myState;
        Devices myDevice;

        public enum States {IDLE, READY, RUN}
        public enum Devices {CPU, GPU}

        public Manager()
        {
            data = Data.get();
            solver = new Solver();
            gpu = new GPGPU();
            postprocess = new Postprocess();
            

            myState = States.IDLE;
            myDevice = Devices.CPU;
        }

        public bool setGeometry()
        {
            if(data.mapping())
            {
                myState = States.READY;
                return true;
            }

            return false;
        }

        public bool setFilePath()
        {
            if(data.setFilePath())
            {
                return true;
            }

            return false;
        }

        /** @brief Form1에 로드한 Geometry 정보를 출력하는 메소드\n
        * 읽어온 Bitmap 정보가 제대로 map 배열에 저장되었는지 확인할 수 있도록 Form에 출력함.\n
        * Fluid cell: White\n
        * Solid(Wall) cell: Black\n
        * Inlet(Left) cell: Red\n
        * Inlet(Right) cell: Blue\n
        * Outlet cell: Green\n
        * simulation domain의 크기를 status.text에 출력함 */
        public Bitmap drawTiles()
        {
            Bitmap buffer = new Bitmap(data.size[0], data.size[1]);
            int i, j;

            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                i = index % data.size[0];
                j = index / data.size[0];

                switch (data.map[index])
                {
                    case 0:
                        buffer.SetPixel(i, j, Color.White);
                        break;
                    case 1:
                        buffer.SetPixel(i, j, Color.Black);
                        break;
                    case 2:
                        buffer.SetPixel(i, j, Color.Red);
                        break;
                    case 3:
                        buffer.SetPixel(i, j, Color.Blue);
                        break;
                    case 4:
                        buffer.SetPixel(i, j, Color.Green);
                        break;
                }
            }

            buffer.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return buffer;
        }

        public void setFluidInfo(string Re, string u0)
        {
            double Re_ = Convert.ToDouble(Re);
            double u0_ = Convert.ToDouble(u0);

            data.setFluidInfo(Re_, u0_);
        }

        public void setIteration(decimal iter)
        {
            int iter_ = (int)iter;
            data.setIteration(iter_);
        }

        public void setExchangeRate(decimal xRate)
        {
            double xRate_ = (double)xRate;
            data.setExchangeRate(xRate_);
        }

        public void setSimulationMode(bool steady, bool optimal)
        {
            // 주작해야함
            bool st = steady;
            bool opt = optimal;

            if (optimal) st = true;

            data.setSimulationMode(st, opt);
        }

        public void setFileName(string name)
        {
            if(!string.IsNullOrWhiteSpace(name))
                data.setFileName(name);
        }

        public void setCriteria(string crit)
        {
            double crit_ = Convert.ToDouble(crit);

            if(crit_ <= 0.1d)
            {
                data.criteria = crit_;
            }
        }

        public void setDevice(bool cpu, bool gpu)
        {
            if(cpu)
            {
                myDevice = Devices.CPU;
            }
            if (gpu)
            {
                myDevice = Devices.GPU;
            }
        }

        public void showResidue()
        {
            double residue = data.diff[0];
            double crit = data.criteria;
            cCb(residue, crit);
        }

        #region Run
        public void Run()
        {
            if(myState == States.READY)
            {
                switch (myDevice)
                {
                    case Devices.CPU:
                        CPU_RUN();
                        break;
                    case Devices.GPU:
                        GPU_RUN();
                        break;
                }
            }
        }

        private void CPU_RUN()
        {
            myState = States.RUN;

            if (data.optimalRun)
            {
                CPU_RunOptimal();
            }
            else
            {
                CPU_RunSimulate();
            }
        }

        private void CPU_RunOptimal()
        {
            for (data.curIterOptimal = 0; data.curIterOptimal < data.iteration; data.curIterOptimal++)
            {
                CPU_RunSimulate();
                solver.optimize();
                post(data.optimalRun);
                solver.resetAll();
            }
        }

        private void CPU_RunSimulate()
        {
            bool run = true;
            data.curIterSim = 1;

            while (run && myState == States.RUN)
            {
                solver.collision();
                solver.streaming(data.fout, data.fin, 0);
                solver.bounceback();
                solver.boundary();
                solver.macroscopic();

                bool checker = data.curIterSim % 50 == 0;
                run = solver.getError(checker);
                if(checker) { showResidue(); }

                data.curIterSim++;
            }
        }

        private void GPU_RUN()
        {
            myState = States.RUN;

            if(data.optimalRun)
            {
                GPU_RunOptimal();
            }
            else
            {
                GPU_RunSimulate();
            }
        }

        private void GPU_RunOptimal()
        {
            for (data.curIterOptimal = 0; data.curIterOptimal < data.iteration; data.curIterOptimal++)
            {
                GPU_RunSimulate();

                solver.optimize();
                post(data.optimalRun);
                disp();
                gpu.resetAll();
            }
        }

        private void GPU_RunSimulate()
        {
            bool run = true;
            data.curIterSim = 1;
            gpu.init();

            while (run && myState == States.RUN)
            {
                gpu.run();

                bool checker = data.curIterSim % 50 == 0;
                run = gpu.getError(checker);
                if (checker) { showResidue(); }
                data.curIterSim++;
            }
            post(data.optimalRun);
        }
        #endregion

        public void Stop()
        {
            myState = States.READY;
        }

        /** @brief textBox4.text에 시뮬레이션 결과로 Inlet과 Outlet의 Pressure Difference를 출력함. */
        private void disp()
        {
            double pd = 0;

            for (int j = 0; j < data.size[1]; j++)
            {
                for (int i = 0; i < data.size[0]; i++)
                {
                    if (data.map[i + data.size[0] * j] == 2) // inlet
                    {
                        pd += data.density[i + data.size[0] * j];
                    }
                    if (data.map[i + data.size[0] * j] == 4) // outlet
                    {
                        pd -= data.density[i + data.size[0] * j];
                    }
                }
            }

            // textBox4.Text += pd + "\r\n";
        }

        /** @brief 후처리를 수행하는 메소드\n
        * @param opti True인 경우 Shape Optimization을 수행하는 경우 LBM 결과와 함께 Geometry 정보를 Bitmap 파일로 출력한다.*/
        private void post(bool opti)
        {
            if (opti)
            {
                postprocess.saveFiles();
                postprocess.saveImages();
            }
            else
            {
                postprocess.saveFiles();
            }
        }
    }
}
