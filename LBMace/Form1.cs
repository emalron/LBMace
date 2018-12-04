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

/** @mainpage LBMace
* @section Intro 소개
* 본 어플리케이션에서 유동장은 Lattice Boltzmann Method를 이용하여 시뮬레이트되며, Shape Optimization은 Heuristic Optimality를 이용하여 수행 된다.\n
* 또한 사용자는 유동장의 시뮬레이션은 CPU의 Single Core로 연산하거나 GPU를 이용하여 병렬 연산 할 수 있다.\n
* 유동장 시뮬레이션의 후처리는 Paraview를 이용할 수 있도록 VTK 포맷으로 저장되며, Shape Optimization을 수행한 경우 변경된 Geometry를 Bitmap 포맷으로 확인 할 수 있도록 하였다.\n
* @section LBM Lattice Boltzmann Method
* Lattice Boltzmann Method는 희박 기체의 운동을 묘사하는 Boltzmann Equation을 이산화하여 거시적인 유동장을 시뮬레이션하는 기법이다.
* - D2Q9: Phase space에 존재하는 분자들의 Distribution Function은 9방향으로 이산화되어 있다.\n\n
* - Single Relaxation Time: 분자들이 서로 충돌 한 후 H-Function이 최소화된 Maxwellian이 되는데 걸리는 시간을 의미하는 Relaxation Time은 본래 Scattering Matrix로 표현되지만 본 어플리케이션에서는 단일 실수값으로 표현하는 Single Relaxation Time 모델을 사용함.\n\n
* - Full-Way Bounce-Back: 벽면에서 No-Slip condition을 구현하기 위해 벽면에서 분자들의 방향이 완전지 반대 방향으로 바뀌는 모델을 적용함.\n\n
* - Zou and He Boundary Condition: Streaming Step의 특성 상 경계면에선 계산 할 수 없는 Unknown Distribution Function이 발생하기 마련이다. Inlet에서 Constant Velocity의 조건을 만족시키기 위해 D2Q9 SRT 모델을 적용한 경우에도 잘 맞는다고 알려진 Zou and He 모델을 적용하였다.\n\n
* - Interpolated Boundary Condition: Outlet에서 Open-ended 조건을 만족시키기 위해 인접한 cell의 distribution function의 값을 가져오는 모델을 적용함.
* - Strain Rate Tensor: Strain rate tensor의 계산은 Distribution Function으로 직접 연산하였다.\n
* Ref: An Alternative Scheme to Calculate the Strain Rate Tensor for the LES Applications in the LBM, J. Li and Z. Wang, 2010, Mathematical Problems in Engineering
* @section SO Shape Optimization
* 본 어플리케이션에서 적용한 Shape Optimization은 기존의 Level-Set Method가 아닌 Heuristic Optimality를 이용하였다. Wang, et al.의 Heuristic Optimality 과 달리 본 어플리케이션에서는 Fluid-Solid Interface에서 Fluid cell이 가지는 Strain Rate Tensor만을 이용하여 Fluid-Solid 교환을 적용한다.\n
* Solid-Fluid의 교환은 언제나 1:1로 일어나므로 Fluid cell의 전체 Volume이 일정하게 유지된다. 또한 계산비용 감소를 위해 Shape Optimization의 한 Iteration마다 Exchange Rate에 따라 다수의 cell이 변환 된다.\n
* Exchange Rate는 전체 Interface cell의 4%가 적절하다고 알려져 있다(Wang, et al, 2010)
* @section Meta 작성자
* @author 박정민 (pjm2108@naver.com)
* @date 2016-07-14
*/

namespace LBMace
{
    /** @brief LBMace의 구동부와 GUI를 포함하고 있는 클래스\n
    * User interface를 이용하여 시뮬레이션을 세팅하고 연산 수행 명령을 내리는 역할을 한다\n
    * 연산은 Solver, GPGPU 에서 수행하며 후처리는 Postprocess 클래스에서 수행됨. */

    public partial class Form1 : Form
    {
        /** @brief data 클래스 선언 */
        Data data;

        /** @brief Solver 클래스 선언 */
        Solver solver;

        /** @brief Postprocess 클래스 선언 */
        Postprocess postprocess;

        /** @brief GPGPU 클래스 선언 */
        GPGPU gpu;

        #region Form controls
        public Form1()
        {
            InitializeComponent();

            data = Data.get();
            solver = new Solver();
            postprocess = new Postprocess();
            gpu = new GPGPU();

            label3.Text = data.savePath;
            textBox2.Text = data.resultFileName;
        }

        /** @brief 1. Geometry의 Load Geometry 버튼\n
        * data.mapping()을 호출하여 Bitmap 포맷의 Geometry 정보를 불러옴\n
        * 불러온 Geometry 정보를 GUI에 표현함 */
        private void button_Init_Click_1(object sender, EventArgs e)
        {
            if (data.mapping())
            {
                drawTiles();

                button_Init.Enabled = false;
                button1.Enabled = true;
            }
        }

        /** @brief CPU Run 버튼\n
        * simulate() 메소드를 호출하여 CPU로 시뮬레이트함 */
        private void button_CPU_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(postprocess.Filepath);

            button_Init.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button_CPU.Enabled = false;
            button_Parallel.Enabled = false;

            simulate(steadyChk.Checked, optiChk.Checked);
            textBox4.Text = postprocess.iter.ToString() + "th, " +  data.criteria[0].ToString();
        }

        /** @brief GPGPU Mode 버튼\n
        * gpuSim() 메소드를 호출하여 GPU로 시뮬레이트함 */
        private void button_Parallel_Click(object sender, EventArgs e)
        {
            // System.Diagnostics.Process.Start(postprocess.Filepath);

            button_Init.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button_CPU.Enabled = false;
            button_Parallel.Enabled = false;

            gpu.init();
            gpuSim(steadyChk.Checked, optiChk.Checked);

            // textBox4.Text = postprocess.iter.ToString() + "th, " + data.criteria[0].ToString();
            // textBox4.Text = data.sb.ToString();
        }

        /** @brief optiChk 버튼\n
        * Shape Optimization 계산을 수행할지 여부를 결정함. true=On, false=Off */
        private void optiChk_CheckedChanged(object sender, EventArgs e)
        {
            if (optiChk.Checked)
            {
                steadyChk.Checked = true;
            }
        }

        /** @brief steadyChk 버튼\n
        * 시뮬레이션을 Steady-state에 도달 할 때까지 지속할지 여부를 결정함. true=On, false=Off */
        private void steadyChk_CheckedChanged(object sender, EventArgs e)
        {
            if (!steadyChk.Checked)
            {
                optiChk.Checked = false;
            }
        }

        /** @brief 2. Properties의 Set Properties 버튼\n
        * Reynolds #와 Initial Velocity를 설정하여 Relaxation Time을 계산한다. */
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;

            setVelocity();
        }
         
        /** @brief 3. Simulation의 Simulation data 버튼\n
        * 시뮬레이션 동작 모드(steady-state, shape optimization)을 결정하고\n
        * Exchange Rate와 결과 파일의 이름과 경로를 설정함 */
        private void button2_Click(object sender, EventArgs e)
        {
            postprocess.setMeta(data.savePath, data.resultFileName);
            data.xrate = (int)numericXrate.Value;

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

        /** @brief Reynolds #와 Initial Velocity를 data 클래스에 등록하는 메소드.\n
        * LBM의 Stability는 Dynamic Viscosity에 직접적인 영향을 받으며, LBM에서 Viscosity는 Tuning Parameter로 알려져있다.\n
        * 또한 LBM에서의 시간, 거리, 무게는 lattice unit으로 차원변환되어 있으므로 initial velocity를 조정하여 viscosity가 안정 범위에 오도록 튜닝할 수 있다.\n
        */
        private void setVelocity()
        {
            double Re, u0;
            Re = Convert.ToDouble(tb_Re.Text);
            u0 = Convert.ToDouble(tb_u0.Text);

            data.relaxationTime(Re, u0);

            updateStatus();
        }

        /** @brief Simulation data의 결과를 표시하는 메소드.\n
        * status.text에 Simulation data의 결과를 표시하며, 해당 데이터는 싱글턴 data 클래스로부터 획득하므로 표시되는 결과가 시뮬레이션에 사용되는 결과이다. */
        private void updateStatus()
        {
            status.Text = "";
            status.Text += "Reynolds #: " + data.Re + "\r\n";
            status.Text += "Initial velocity: " + data.u0 + "\r\n";
            status.Text += "Relaxation Time: " + data.tau + "\r\n";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path_ = data.getFilePath();
            label3.Text = path_;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string plain1 = "file name, ex)";
            string filename = textBox2.Text;
            string ext = "_0.vtx";

            string text_ = String.Format("{0} {1}{2}", plain1, filename, ext);

            label7.Text = text_;
        }
    }
}
