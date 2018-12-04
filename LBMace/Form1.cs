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
        Manager manager;

        public Form1()
        {
            InitializeComponent();

            data = Data.get();
            manager = new Manager();
            updateForm(true);
        }

        private void updateForm(bool changed)
        {
            if (changed)
            {
                manager.setFluidInfo(tb_Re.Text, tb_u0.Text);
                manager.setIteration(numericIter.Value);
                manager.setExchangeRate(numericXrate.Value);
                manager.setSimulationMode(steadyChk.Checked, optiChk.Checked);
                manager.setFileName(textBox2.Text);
                manager.setDevice(radioButton1.Checked, radioButton2.Checked);
                manager.setCriteria(textBox1.Text);

                // update Form Context
                label3.Text = data.savePath;
                string plain1 = "file name, ex)";
                string filename = textBox2.Text;
                string ext = "_0.vtx";
                string text_ = String.Format("{0} {1}{2}", plain1, filename, ext);
                label7.Text = text_;
                steadyChk.Checked = data.steadyRun;
                optiChk.Checked = data.optimalRun;

                richTextBox1.Text = "";
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

        private void button_Init_Click_1(object sender, EventArgs e)
        {
            if (manager.setGeometry())
            {
                // show input geometry
                Bitmap input = manager.drawTiles();
                pictureBox1.Image = input;

                // foolproof
                button1.Enabled = true;
                button_RUN.Enabled = true;
                button_STOP.Enabled = true;
            }
        }

        private void button_CPU_Click(object sender, EventArgs e)
        {
            // System.Diagnostics.Process.Start(postprocess.Filepath);
            button_RUN.Enabled = false;
            button_STOP.Enabled = true;

            manager.Run();
        }

        private void button_Parallel_Click(object sender, EventArgs e)
        {
            // System.Diagnostics.Process.Start(postprocess.Filepath);
            button_RUN.Enabled = true;
            button_STOP.Enabled = false;

            manager.Stop();
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
            button_RUN.Enabled = true;
            button_STOP.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // set a folder to save result files
            if(manager.setFilePath())
            {
                updateForm(true);
            }
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

        private void optiChk_CheckedChanged_1(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            updateForm(true);
        }
    }
}
