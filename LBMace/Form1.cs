using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace LBMace
{
    /** @brief LBMace의 구동부와 GUI를 포함하고 있는 클래스\n
    * User interface를 이용하여 시뮬레이션을 세팅하고 연산 수행 명령을 내리는 역할을 한다\n
    * 연산은 Solver, GPGPU 에서 수행하며 후처리는 Postprocess 클래스에서 수행됨. */

    public partial class Form1 : Form
    {
        Data data;
        GPGPU gpu;
        Solver solver;
        Postprocess postprocess;

        #region Form controls
        public Form1()
        {
            InitializeComponent();

            data = Data.get();
            gpu = new GPGPU();
            solver = new Solver();
            postprocess = new Postprocess();

            updateForm(true);
        }

        private void button_Init_Click_1(object sender, EventArgs e)
        {
            if (data.mapping())
            {
                drawTiles();

                // foolproof
                button_Init.Enabled = false;
                button1.Enabled = true;
                button_CPU.Enabled = true;
                button_Parallel.Enabled = true;
            }
        }

        private void button_CPU_Click(object sender, EventArgs e)
        {
            // System.Diagnostics.Process.Start(postprocess.Filepath);

            button_Init.Enabled = true;
            button1.Enabled = false;
            button_CPU.Enabled = false;
            button_Parallel.Enabled = false;

            simulate(steadyChk.Checked, optiChk.Checked);
        }

        private void button_Parallel_Click(object sender, EventArgs e)
        {
            // System.Diagnostics.Process.Start(postprocess.Filepath);

            button_Init.Enabled = true;
            button1.Enabled = false;
            button_CPU.Enabled = false;
            button_Parallel.Enabled = false;

            gpu.init();
            gpuSim(steadyChk.Checked, optiChk.Checked);
        }

        private void optiChk_CheckedChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void steadyChk_CheckedChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            updateForm(true);
        }
         
        private void button2_Click(object sender, EventArgs e)
        {
            postprocess.setMeta(data.savePath, data.resultFileName);

            button_CPU.Enabled = true;
            button_Parallel.Enabled = true;
        }

        #endregion

        /** @brief CPU로 시뮬레이트하는 메소드\n
        * LBM은 Collision → Streaming → Bounce-Back → Boundary Condition → Macroscopic Variables 순으로 연산한다.\n
        * Shape Optimization은 LBM의 결과가 Steady-state에 도달한 이후에 수행되며, Interface Search → Wall Control 순서로 수행됨.\n
        * @param steady true일 경우 steady-state에 도달할 때까지 반복 연산함
        * @param opti true일 경우 steady-state에 도달한 이후 Heuristic optimality를 이용하여 Shape optimization을 수행함*/
        private void simulate(bool steady, bool opti)
        {
            bool run = true;
            int cnt;

            if (opti)
            {
                for (postprocess.iter = 0; postprocess.iter < (int)numericIter.Value; postprocess.iter++)
                {
                    run = true;
                    cnt = 0;
                    while (run)
                    {
                        solver.collision();
                        solver.streaming(data.fout, data.fin, 0);
                        solver.bounceback();
                        solver.boundary();
                        solver.macroscopic();
                        run = solver.getError((cnt % 100 == 0) && (cnt > 1));
                        cnt++;
                    }
                    solver.optimize();
                    post(opti);
                    solver.resetAll();
                }
            }
            else {
                if (steady)
                {
                    postprocess.iter = 0;
                    while (run)
                    {
                        solver.collision();
                        solver.streaming(data.fout, data.fin, 0);
                        solver.bounceback();
                        solver.boundary();
                        solver.macroscopic();
                        run = solver.getError((postprocess.iter % 100 == 0) && (postprocess.iter > 1));
                        postprocess.iter++;
                    }
                }
                else
                {
                    for (postprocess.iter = 0; postprocess.iter < (int)numericIter.Value; postprocess.iter++)
                    {
                        solver.collision();
                        solver.streaming(data.fout, data.fin, 0);
                        solver.bounceback();
                        solver.boundary();
                        solver.macroscopic();
                    }
                    solver.getError(true);
                }
            }
        }

        /** @brief GPU로 시뮬레이트하는 메소드\n
        * LBM은 Collision → Streaming → Bounce-Back → Boundary Condition → Macroscopic Variables 순으로 병렬 연산한다.\n
        * Shape Optimization은 LBM의 결과가 Steady-state에 도달한 이후에 수행되며, Interface Search → Wall Control 순서로 수행됨.\n
        * @param steady true일 경우 steady-state에 도달할 때까지 반복 연산함
        * @param opti true일 경우 steady-state에 도달한 이후 Heuristic optimality를 이용하여 Shape optimization을 수행함*/
        private void gpuSim(bool steady, bool opti)
        {
            bool run = true;
            int cnt = 0;

            if (opti)
            {
                for (postprocess.iter = 0; postprocess.iter < (int)numericIter.Value; postprocess.iter++)
                {
                    cnt = 0;
                    run = true;
                    while (run)
                    {
                      gpu.run();
                      run = gpu.getError((cnt % 100 == 0) && (cnt > 1));
                      cnt++;
                    }

                    solver.optimize();
                    post(opti);
                    disp();
                    gpu.resetAll();
                }
            }
            else
            {
                if (steady)
                {
                    postprocess.iter = 0;
                    while (run)
                    {
                        gpu.run();
                        run = gpu.getError((postprocess.iter % 50 == 0) && (postprocess.iter > 1));
                        postprocess.iter++;
                    }
                    post(opti);

                }
                else
                {
                    for (postprocess.iter = 0; postprocess.iter < (int)numericIter.Value; postprocess.iter++)
                    {
                        gpu.run();
                        if(postprocess.iter % 50 == 0)
                            post(opti);
                    }
                    gpu.getError(true);
                }
            }
        }

        /** @brief textBox4.text에 시뮬레이션 결과로 Inlet과 Outlet의 Pressure Difference를 출력함. */
        private void disp()
        {
            double pd = 0;

            for (int j = 0; j < data.size[1]; j++) {
                for (int i = 0; i < data.size[0]; i++)
                {
                    if(data.map[i + data.size[0] * j] == 2) // inlet
                    {
                        pd += data.density[i + data.size[0] * j];
                    }
                    if(data.map[i + data.size[0] * j] == 4) // outlet
                    {
                        pd -= data.density[i + data.size[0] * j];
                    }
                }
            }

            textBox4.Text += pd + "\r\n";
        }

        /** @brief 후처리를 수행하는 메소드\n
        * @param opti True인 경우 Shape Optimization을 수행하는 경우 LBM 결과와 함께 Geometry 정보를 Bitmap 파일로 출력한다.*/
        private void post(bool opti)
        {
            if(opti)
            {
                postprocess.saveFiles();
                postprocess.saveImages();
            }
            else
            {
                postprocess.saveFiles();
            }
        }

        /** @brief Form1에 로드한 Geometry 정보를 출력하는 메소드\n
        * 읽어온 Bitmap 정보가 제대로 map 배열에 저장되었는지 확인할 수 있도록 Form에 출력함.\n
        * Fluid cell: White\n
        * Solid(Wall) cell: Black\n
        * Inlet(Left) cell: Red\n
        * Inlet(Right) cell: Blue\n
        * Outlet cell: Green\n
        * simulation domain의 크기를 status.text에 출력함 */
        private void drawTiles()
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


            pictureBox1.Width = data.size[0];
            pictureBox1.Height = data.size[1];
            pictureBox1.Image = buffer;

            status.Text += "\r\nthe size is " + data.size[0].ToString() + "x" + data.size[1].ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // set a folder to save result files
            data.setFilePath();

            updateForm(true);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void tb_u0_TextChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void tb_Re_TextChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void numericIter_ValueChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void steadyChk_CheckedChanged_1(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void numericXrate_ValueChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void updateForm(bool changed)
        {
            if (changed)
            {
                double Re, u0;
                Re = Convert.ToDouble(tb_Re.Text);
                u0 = Convert.ToDouble(tb_u0.Text);
                data.setFluidInfo(Re, u0);
                data.setIteration((int)numericIter.Value);
                data.setExchangeRate((double)numericXrate.Value);
                data.setSimulationMode(steadyChk.Checked, optiChk.Checked);
                data.setFileName(textBox2.Text);
                postprocess.setMeta(data.savePath, data.saveFileName);
                label3.Text = data.savePath;

                string plain1 = "file name, ex)";
                string filename = textBox2.Text;
                string ext = "_0.vtx";
                string text_ = String.Format("{0} {1}{2}", plain1, filename, ext);
                label7.Text = text_;

                foreach (string line in data.getSimulationInfo())
                {
                    if (line.StartsWith("#"))
                    {
                        richTextBox1.SelectionColor = Color.Red;
                    }
                    else
                    {
                        richTextBox1.SelectionColor = Color.Black;
                    }

                    richTextBox1.AppendText(String.Format("{0}\r\n", line));
                }
            }
        }
    }
}
