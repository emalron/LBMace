﻿namespace LBMace
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_CPU = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_Parallel = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button_Init = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.tb_u0 = new System.Windows.Forms.TextBox();
            this.status = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_Re = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.numericXrate = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericIter = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.optiChk = new System.Windows.Forms.CheckBox();
            this.steadyChk = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericXrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIter)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_CPU
            // 
            this.button_CPU.Enabled = false;
            this.button_CPU.Location = new System.Drawing.Point(7, 435);
            this.button_CPU.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_CPU.Name = "button_CPU";
            this.button_CPU.Size = new System.Drawing.Size(472, 29);
            this.button_CPU.TabIndex = 13;
            this.button_CPU.Text = "CPU Run";
            this.button_CPU.UseVisualStyleBackColor = true;
            this.button_CPU.Click += new System.EventHandler(this.button_CPU_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(240, 224);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(169, 15);
            this.label7.TabIndex = 17;
            this.label7.Text = "file name, ex) fluid_0.vtk";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(242, 242);
            this.textBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(221, 25);
            this.textBox2.TabIndex = 11;
            this.textBox2.Text = "fluid";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 289);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(186, 15);
            this.label8.TabIndex = 20;
            this.label8.Text = "the results will be saved in ";
            // 
            // button_Parallel
            // 
            this.button_Parallel.Enabled = false;
            this.button_Parallel.Location = new System.Drawing.Point(7, 471);
            this.button_Parallel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Parallel.Name = "button_Parallel";
            this.button_Parallel.Size = new System.Drawing.Size(472, 29);
            this.button_Parallel.TabIndex = 14;
            this.button_Parallel.Text = "GPGPU Mode";
            this.button_Parallel.UseVisualStyleBackColor = true;
            this.button_Parallel.Click += new System.EventHandler(this.button_Parallel_Click);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(7, 69);
            this.textBox4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox4.Size = new System.Drawing.Size(253, 430);
            this.textBox4.TabIndex = 24;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(14, 15);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(495, 540);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_Init);
            this.tabPage1.Controls.Add(this.pictureBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(487, 511);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "1. Geometry";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button_Init
            // 
            this.button_Init.Location = new System.Drawing.Point(7, 8);
            this.button_Init.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Init.Name = "button_Init";
            this.button_Init.Size = new System.Drawing.Size(128, 29);
            this.button_Init.TabIndex = 2;
            this.button_Init.Text = "Load Geometry";
            this.button_Init.UseVisualStyleBackColor = true;
            this.button_Init.Click += new System.EventHandler(this.button_Init_Click_1);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(7, 44);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(472, 445);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.tb_u0);
            this.tabPage2.Controls.Add(this.status);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.tb_Re);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Size = new System.Drawing.Size(487, 511);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "2. Properties";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(7, 8);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 29);
            this.button1.TabIndex = 5;
            this.button1.Text = "Set Properties";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tb_u0
            // 
            this.tb_u0.Location = new System.Drawing.Point(130, 70);
            this.tb_u0.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tb_u0.Name = "tb_u0";
            this.tb_u0.Size = new System.Drawing.Size(114, 25);
            this.tb_u0.TabIndex = 4;
            this.tb_u0.Text = "0.08";
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(7, 294);
            this.status.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.status.Multiline = true;
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(471, 205);
            this.status.TabIndex = 5;
            this.status.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(128, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 15);
            this.label1.TabIndex = 30;
            this.label1.Text = "Initial Velocity, u0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 15);
            this.label2.TabIndex = 31;
            this.label2.Text = "Reynolds #, Re";
            // 
            // tb_Re
            // 
            this.tb_Re.Location = new System.Drawing.Point(9, 70);
            this.tb_Re.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tb_Re.Name = "tb_Re";
            this.tb_Re.Size = new System.Drawing.Size(114, 25);
            this.tb_Re.TabIndex = 3;
            this.tb_Re.Text = "20";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button3);
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.numericXrate);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.numericIter);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.textBox2);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.optiChk);
            this.tabPage3.Controls.Add(this.steadyChk);
            this.tabPage3.Controls.Add(this.button_CPU);
            this.tabPage3.Controls.Add(this.button_Parallel);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage3.Size = new System.Drawing.Size(487, 511);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "3. Simulation";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(8, 242);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(205, 25);
            this.button3.TabIndex = 37;
            this.button3.Text = "Choose a folder to save";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(7, 8);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(133, 29);
            this.button2.TabIndex = 12;
            this.button2.Text = "Simulation Setup";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 146);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(108, 15);
            this.label10.TabIndex = 35;
            this.label10.Text = "Exchange Rate";
            // 
            // numericXrate
            // 
            this.numericXrate.Location = new System.Drawing.Point(9, 165);
            this.numericXrate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numericXrate.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericXrate.Name = "numericXrate";
            this.numericXrate.Size = new System.Drawing.Size(114, 25);
            this.numericXrate.TabIndex = 9;
            this.numericXrate.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(188, 289);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 15);
            this.label3.TabIndex = 36;
            this.label3.Text = "d:\\result\\fluid_0.vtk";
            // 
            // numericIter
            // 
            this.numericIter.Location = new System.Drawing.Point(7, 104);
            this.numericIter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numericIter.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericIter.Name = "numericIter";
            this.numericIter.Size = new System.Drawing.Size(114, 25);
            this.numericIter.TabIndex = 8;
            this.numericIter.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 15);
            this.label4.TabIndex = 30;
            this.label4.Text = "iterations";
            // 
            // optiChk
            // 
            this.optiChk.AutoSize = true;
            this.optiChk.Checked = true;
            this.optiChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optiChk.Location = new System.Drawing.Point(242, 105);
            this.optiChk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.optiChk.Name = "optiChk";
            this.optiChk.Size = new System.Drawing.Size(132, 19);
            this.optiChk.TabIndex = 7;
            this.optiChk.Text = "shape optimizer";
            this.optiChk.UseVisualStyleBackColor = true;
            // 
            // steadyChk
            // 
            this.steadyChk.AutoSize = true;
            this.steadyChk.Checked = true;
            this.steadyChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.steadyChk.Location = new System.Drawing.Point(128, 105);
            this.steadyChk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.steadyChk.Name = "steadyChk";
            this.steadyChk.Size = new System.Drawing.Size(113, 19);
            this.steadyChk.TabIndex = 6;
            this.steadyChk.Text = "steady-state";
            this.steadyChk.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.textBox4);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage4.Size = new System.Drawing.Size(487, 511);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "4. Results";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 565);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "LBMace";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericXrate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIter)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_CPU;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_Parallel;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tb_Re;
        private System.Windows.Forms.NumericUpDown numericXrate;
        private System.Windows.Forms.Button button_Init;
        private System.Windows.Forms.CheckBox optiChk;
        private System.Windows.Forms.TextBox tb_u0;
        private System.Windows.Forms.CheckBox steadyChk;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericIter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        public System.Windows.Forms.TextBox status;
        public System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

