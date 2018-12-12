using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace LBMace
{
    class Data
    {
        // singleton...
        private static Data data;
        public static Data get()
        {
            if (data == null)
            {
                data = new Data();
            }
            return data;
        }

        private Data()
        {
            /** 2D에 대한 변수 설정 */
            inletID = new int[4];
            size = new int[2];
            sb = new StringBuilder();

            Re_ = 20d;

            savePath_ = System.Environment.CurrentDirectory;
            saveFileName_ = "fluid";
        }

        #region internal variables
        // distribute functions의 초기값과 다음 스텝 값
        public double[] fin, fout;
        
        // distribution functions의 zeroth moment, density
        public double[] density;
        
        // distribution function의 first moment, velocity
        public double[] ux, uy;
        
        // 수렴도 계산에 사용됨
        public double[] diff;
        public double[] up, vp;
        public double criteria;

        // heuristic criteria: strain rate tensor, dynamic pressure
        public double[] strain, dynamic;
        
        // 
        public StringBuilder sb;
        #endregion

        #region Simulation setting
        // Geometry 정보. map, size, inletID
        public int[] map, size, inletID;
        // inlet의 길이를 계산함
        private double inletLeng;

        // 초기 CFL을 결정하는 정보 초기속도 u0, 레이놀즈 수 Re, 교환비 xrate
        private double u0_, Re_, xrate_;
        public double u0
        {
            get
            {
                return u0_;
            }
        }
        public double Re
        {
            get
            {
                return Re_;
            }
        }
        public double xrate
        {
            get
            {
                return xrate_;
            }
        }

        // relaxation time을 계산함. BGK Model
        public double tau
        {
            get
            {
                return u0 * inletLeng / (this.Re_) * 3.0d + 0.5d;
            }
        }

        // optimization 및 simulation 횟수
        private int iteration_, curIterOptimal_, curIterSim_;
        public int iteration {
            get
            {
                return iteration_;
            }
        }
        public int curIterOptimal
        {
            get
            {
                return curIterOptimal_;
            }
            set
            {
                curIterOptimal_ = value;
            }
        }
        public int curIterSim
        {
            get
            {
                return curIterSim_;
            }
            set
            {
                curIterSim_ = value;
            }
        }

        // Solver 동작 모드
        private bool steadyRun_, optimalRun_;

        public bool steadyRun
        {
            get
            {
                return steadyRun_;
            }
        }

        public bool optimalRun
        {
            get
            {
                return optimalRun_;
            }
        }

        // GPGPU device 정보
        public List<string> devices
        {
            get
            {
                return GPGPU.getDeviceInfo(myPlatform_);
            }
        }

        public List<string> platforms
        {
            get
            {
                return GPGPU.getPlatformInfo();
            }
        }

        private int myDevice_;
        public int myDevice
        {
            get
            {
                return myDevice_;
            }
        }

        private int myPlatform_;
        public int myPlatform
        {
            get
            {
                return myPlatform_;
            }
        }

        // file 저장 정보
        private string savePath_;
        public string savePath
        {
            get
            {
                return savePath_;
            }
        }

        private string saveFileName_;
        public string saveFileName
        {
            get
            {
                return saveFileName_;
            }
        }

        public string resultFileName
        {
            get
            {
                return String.Format(@"{0}\{1}", savePath_, saveFileName_);
            }
        }
        #endregion

        public void setFluidInfo(double Re, double u0)
        {
            this.Re_ = Re;
            this.u0_ = u0;
        }

        public void setExchangeRate(double rate)
        {
            this.xrate_ = rate;
        }

        public void setIteration(int iter)
        {
            this.iteration_ = iter;
        }

        public void setSimulationMode(bool steady, bool opti)
        {
            this.steadyRun_ = steady;
            this.optimalRun_ = opti;
        }

        public void setGPUDevice(int platform, int device)
        {
            this.myPlatform_ = platform;
            this.myDevice_ = device;
        }

        private void getInletLength()
        {
            double x0, x1, y0, y1;
            x0 = inletID[0] % size[0];
            x1 = inletID[1] % size[0];
            y0 = inletID[0] / size[0];
            y1 = inletID[1] / size[0];

            inletLeng = Math.Abs(Math.Sqrt(Math.Pow((x1 - x0), 2) + Math.Pow((y1 - y0), 2)));
            
            // viscosity = 0.04d;
            // tau = 3d * viscosity + 0.5d;
            
            // u0 = ((tau-0.5d)/3d) * this.Re / inletLeng;
        }

        // Bitmap으로부터 map 데이터를 읽음
        public bool mapping()
        {
            Bitmap tiles_ = new Bitmap(10,10);

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

        /** @brief 각종 시뮬레이션 관련 변수 선언 및 초기화
        */
        public void init()
        {
            int area = size[0] * size[1];

            // inlet length를 계산함.
            getInletLength();

            /* D2Q9 scheme의 weighting factor */
            double[] weight = new double[9] { 4d / 9d, 1d / 9d, 1d / 9d, 1d / 9d, 1d / 9d, 1d / 36d, 1d / 36d, 1d / 36d, 1d / 36d };

            fin = new double[area * 9];
            fout = new double[area * 9];
            density = new double[area];
            ux = new double[area];
            uy = new double[area];
            up = new double[area];
            vp = new double[area];
            diff = new double[area];
            strain = new double[area];
            dynamic = new double[area];

            for (int index = 0; index < area; index++)
            {
                density[index] = 1d;
                ux[index] = 0;
                uy[index] = 0;
                up[index] = 0;

                for (int n = 0; n < 9; n++)
                {
                    fin[n + 9 * index] = weight[n];
                    fout[n + 9 * index] = weight[n];
                }
            }
        }

        /** @brief Geometry를 Bitmap으로부터 로딩하는 method
        * @return 성공 여부
        */
        private bool loadFile(ref Bitmap tiles)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".bmp";
            ofd.Filter = "Map file (.bmp)|*.bmp";

            ofd.ShowDialog();

            if(ofd.FileName != "")
            {
                tiles = new Bitmap(Image.FromFile(ofd.FileName));
                tiles.RotateFlip(RotateFlipType.RotateNoneFlipY);
                return true;
            }
            
            return false;
        }

        /** @brief 로딩한 geometry의 크기를 simulation domain의 크기로 설정함
        * @return 2D simulation domain의 크기를 가진 int 배열
        */
        private int[] sizing(Bitmap tiles)
        {
            int[] lengs = new int[2];

            lengs[0] = tiles.Width;
            lengs[1] = tiles.Height;

            return lengs;
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
            List<int> result = new List<int>();

            var output =
                from j in Enumerable.Range(0, tiles.Height)
                from i in Enumerable.Range(0, tiles.Width)
                select tiles.GetPixel(i, j);
            
            foreach(var tile in output)
            {
                if (tile == Color.FromArgb(255, 255, 255, 255))
                {
                    result.Add(0);
                }
                else if (tile == Color.FromArgb(255, 0, 0, 0))
                {
                    result.Add(1);
                }
                else if (tile == Color.FromArgb(255, 255, 0, 0))
                {
                    result.Add(2);
                }
                else if (tile == Color.FromArgb(255,0,0,255))
                {
                    result.Add(3);
                }
                else
                {
                    result.Add(4);
                }
            }
            
            return result.ToArray();
        }

        /** @brief inletID 계산을 위한 색상정보 검색
        * @return inletID에 저장될 값
        */
        private int[] counterColor()
        {
            int[] output = new int[4];
            int cntRed, cntBlue;

            cntRed = 0;
            cntBlue = 0;

            for (int index = 0; index < size[0] * size[1]; index++)
            {
                if (map[index] == 2)
                {
                    if(cntRed == 0)
                    {
                        output[0] = index;
                    }
                    output[1] = index;
                    cntRed++;
                }
                if (map[index] == 3)
                {
                    if(cntBlue == 0)
                    {
                        output[2] = index;
                    }
                    output[3] = index;
                    cntBlue++;
                }
            }

            return output;
        }

        public bool setFilePath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    savePath_ = fbd.SelectedPath;
                }

                return true;
            }
        }

        public void setFileName(string name)
        {
            this.saveFileName_ = name;
        }

        public List<string> getSimulationInfo()
        {
            List<string> output = new List<string>();

            // Fluid info
            output.Add("#Fluid info");
            string initVelocity = String.Format("Initial Velocity: {0}", this.u0_.ToString());
            string reynolds = String.Format("Reynolds #: {0}", this.Re_.ToString());
            string chaLength = String.Format("Characteristic Length: {0}", this.inletLeng.ToString());
            string relaxTime = String.Format("Relaxation Time: {0}", this.tau.ToString());

            output.Add(initVelocity);
            output.Add(chaLength);
            output.Add(reynolds);
            output.Add(relaxTime);
            output.Add("");

            // Setting
            output.Add("#Configurations");
            string steady = String.Format("Steady-state mode: {0}", this.steadyRun_.ToString());
            string optimal = String.Format("Optimization mode: {0}", this.optimalRun_.ToString());
            string iteration = String.Format("Iternation: {0}", this.iteration_.ToString());
            string exRate = String.Format("Exchange rate: {0}", this.xrate_.ToString());
            string crit = String.Format("Residue Criteria: {0}", this.criteria.ToString());

            output.Add(steady);
            output.Add(optimal);
            output.Add(iteration);
            output.Add(exRate);
            output.Add(crit);
            output.Add("");

            // Path
            string path = String.Format("Save Path: {0}", savePath_);
            string name = String.Format("File Name: {0}", saveFileName_);

            output.Add(path);
            output.Add(name);
            output.Add("");

            // GPU Devices
            output.Add("#Selected GPGPU Device");
            List<string> platforms = GPGPU.getPlatformInfo();
            List<string> devices = GPGPU.getDeviceInfo(myPlatform_);

            string platform = String.Format("Platform: {0}",  platforms[myPlatform_]);
            string device = String.Format("Device: {0}", devices[myDevice_]);

            output.Add(platform);
            output.Add(device);

            return output;
        }
    }
    
}
