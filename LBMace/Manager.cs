﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

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
        public Devices myDevice;

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

        // Bitmap으로부터 map 데이터를 읽음
        public bool mapping()
        {
            Bitmap tiles_ = new Bitmap(10, 10);

            if (loadFile(ref tiles_))
            {
                size = sizing(tiles_);
                map = getColor(tiles_);
                inletID = counterColor();

                init();
                return true;
            }
            return false;
        }

        private bool loadFile(ref Bitmap tiles)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".bmp";
            ofd.Filter = "Map file (.bmp)|*.bmp";

            ofd.ShowDialog();

            if (ofd.FileName != "")
            {
                tiles = new Bitmap(Image.FromFile(ofd.FileName));
                tiles.RotateFlip(RotateFlipType.RotateNoneFlipY);
                return true;
            }

            return false;
        }

        private int[] sizing(Bitmap tiles)
        {
            int[] lengs_ = new int[2];

            lengs_[0] = tiles.Width;
            lengs_[1] = tiles.Height;

            return lengs_;

        }


        /** @brief 로딩한 Bitmap의 색상정보로부터 각 lattice의 type을 구분함\n
        * White(0): Fluid, \n
        * Black(1): Wall, \n
        * Red(2): Inlet(left), \n
        * Blue(3): Inlet(Right), \n
        * Green(4): Outlet\n
        * @return 2D simulation domain의 lattice 정보
        */
        private int[] getColor(Bitmap tiles)
        {
            List<int> result_ = new List<int>();

            var output_ =
                from j in Enumerable.Range(0, tiles.Height)
                from i in Enumerable.Range(0, tiles.Width)
                select tiles.GetPixel(i, j);
        
            foreach (var tile in output_)
            {
                if (tile == Color.FromArgb(255, 255, 255, 255))
                {
                    result_.Add(0);
                }
                else if (tile == Color.FromArgb(255, 0, 0, 0))
                {
                    result_.Add(1);
                }
                else if (tile == Color.FromArgb(255, 255, 0, 0))
                {
                    result_.Add(2);
                }
                else if (tile == Color.FromArgb(255, 0, 0, 255))
                {
                    result_.Add(3);
                }
                else
                {
                    result_.Add(4);
                }
            }

            return result_.ToArray();
        }

        /** @brief inletID 계산을 위한 색상정보 검색
        * @return inletID에 저장될 값
        */
        private int[] counterColor(int[] map)
        {
            //int[] output = new int[4];
            //int cntRed, cntBlue;

            //cntRed = 0;
            //cntBlue = 0;

            //for (int index = 0; index < size[0] * size[1]; index++)
            //{
            //    if (map[index] == 2)
            //    {
            //        if (cntRed == 0)
            //        {
            //            output[0] = index;
            //        }
            //        output[1] = index;
            //        cntRed++;
            //    }
            //    if (map[index] == 3)
            //    {
            //        if (cntBlue == 0)
            //        {
            //            output[2] = index;
            //        }
            //        output[3] = index;
            //        cntBlue++;
            //    }
            //}

            int[] map_ = map;
            int size_ = map_.Length;
            int[] output_;

            var temp_ =
                from i in Enumerable.Range(0, size_)
                let v = map_[i]
                group i by v into colors
                select colors;

            var inletReds = getArrayFromGroup(temp_, 2);
            var inletBlues = getArrayFromGroup(temp_, 3);
            var outlets = getArrayFromGroup(temp_, 4);

            output_ = new int[4] { inletReds.Min(), inletReds.Max(), inletBlues.Min(), inletBlues.Max() };

            return output_;
        }

        List<int> getArrayFromGroup(object groups, int key)
        {
            List<int> output_ = new List<int>();
            var groups_ = groups as IEnumerable<IGrouping<int, int>>;

            var temp_ =
                from g in groups_
                where g.Key == 2
                from e in g
                select e;

            output_ = temp_.ToList<int>();

            return output_;
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

        public void setDevice(bool cpu, bool gpu, int platform, int device)
        {
            if(cpu)
            {
                myDevice = Devices.CPU;
            }
            if (gpu)
            {
                myDevice = Devices.GPU;
                data.setGPUDevice(platform, device);
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
            data.init();

            while (run && myState == States.RUN)
            {
                solver.collision();
                solver.streaming(data.fout, data.fin, 0);
                solver.bounceback();
                solver.boundary();
                solver.macroscopic();

                bool isTimeToGetResidue = data.curIterSim % 50 == 0;
                run = solver.getError(isTimeToGetResidue);
                if(isTimeToGetResidue) { showResidue(); }

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

                bool isTimeToGetResidue = data.curIterSim % 50 == 0;
                run = gpu.getError(isTimeToGetResidue);
                if (isTimeToGetResidue) { showResidue(); }
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
