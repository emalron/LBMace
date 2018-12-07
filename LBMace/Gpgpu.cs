using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloo;

namespace LBMace
{
    /** @brief 병렬연산을 위해 openCL을 이용하여 GPGPU 연산을 수행하는 클래스\n
    * solver의 LBM 관련 메소드를 GPGPU kernel로 변환함.\n
    * GPGPU 클래스는 뭘하고 있는지 알 수 없으면 건드리지 말 것!
    */
    class GPGPU
    {
        /** @brief HOST 데이터에 접근하기 위한 싱글턴 data 클래스 호출 */
        Data data;

        /** @brief GPGPU의 컨텍스트 선언 */
        ComputeContext ctx;

        /** @brief DEVICE의 int형 배열 선언 data.map 에 해당함 */
        ComputeBuffer<int> gpu_map;

        /** @brief DEVICE의 int형 배열 선언 data.size[0] 에 해당함 */
        ComputeBuffer<int> gpu_lx;                       

        /** @brief DEVICE의 int형 배열 선언 data.size[1] 에 해당함 */
        ComputeBuffer<int> gpu_ly;                       

        /** @brief DEVICE의 int형 배열 선언 data.inletID 에 해당함 */
        ComputeBuffer<int> gpu_inLocal;

        /** @brief DEVICE의 int형 배열 선언 data.size[0]*data.size[1] 에 해당함 */
        ComputeBuffer<int> gpu_number;

        /** @brief DEVICE의 int형 배열 선언 9 * data.size[0]*data.size[1] 에 해당함 */
        ComputeBuffer<int> gpu_number9;

        /** @brief DEVICE의 double 배열 선언 data.u0 에 해당함 */
        ComputeBuffer<double> gpu_u0;

        /** @brief DEVICE의 double 배열 선언 data.fin 에 해당함 */
        ComputeBuffer<double> gpu_fin;

        /** @brief DEVICE의 double 배열 선언 data.fout 에 해당함 */
        ComputeBuffer<double> gpu_fout;

        /** @brief DEVICE의 double 배열 선언 data.density 에 해당함 */
        ComputeBuffer<double> gpu_density;

        /** @brief DEVICE의 double 배열 선언 data.ux 에 해당함 */
        ComputeBuffer<double> gpu_u;

        /** @brief DEVICE의 double 배열 선언 data.uy 에 해당함 */
        ComputeBuffer<double> gpu_v;

        /** @brief DEVICE의 double 배열 선언 1/data.tau 에 해당함 */
        ComputeBuffer<double> gpu_omega;

        /** @brief DEVICE의 double 배열 선언 data.criteria 에 해당함 */
        ComputeBuffer<double> gpu_criteria;

        /** @brief DEVICE의 double 배열 선언 data.up 에 해당함 */
        ComputeBuffer<double> gpu_up;

        /** @brief DEVICE의 double 배열 선언 data.strain 에 해당함 */
        ComputeBuffer<double> gpu_strain;

        /** @brief DEVICE의 double 배열 선언 data.dynamic 에 해당함 */
        ComputeBuffer<double> gpu_dynamic;

        /** @brief DEVICE의 kernel을 수행하는 명령어 queue가 저장됨 */
        ComputeCommandQueue cq;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 collision()에 해당함 */
        ComputeKernel kernelCollision;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 streaming()에 해당함 */
        ComputeKernel kernelStream;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 bounceback()의 일부 기능을 담당함 */
        ComputeKernel kernelSwap;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 bounceback()의 일부 기능을 담당함 */
        ComputeKernel kernelBounceback;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 boundary()에 해당함 */
        ComputeKernel kernelBC;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 macroscopic()에 해당함 */
        ComputeKernel kernelMacro;

        /** @brief DEVICE의 kernel을 선언함, Solver 클래스의 getError()의 일부 기능을 담당함 */
        ComputeKernel KernelMaxima;

        /** @brief DEVICE의 kernel을 컴파일하는 장소 */
        ComputeProgram prog;

        /** @brief DEVICE의 kernel을 실행할 thread의 갯수가 저장된다. data.size[0]*data.size[1] 만큼 */
        long[] worker;

        /** @brief DEVICE의 kernel을 실행할 thread의 갯수가 저장된다. 9*data.size[0]*data.size[1] 만큼 */
        long[] worker9;

        /** @brief DEVICE의 kernel code가 저장 된다 */
        string kernels;

        public GPGPU()
        {
            data = Data.get();

            ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
            ctx = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);

            string deviceInfo = ComputePlatform.Platforms[0].Devices[0].Name;
        }

        public static List<string> getDeviceInfo()
        {
            List<string> output = new List<string>();

            foreach(var platform in ComputePlatform.Platforms)
            {
                output.Add(platform.Name);
            }

            return output;
        }

        /** @brief openCL을 초기화하는 메소드\n
        * 1. 커널 코드를 작성함\n
        * 2. openCL 초기화\n
        * 3. 커널 컴파일, 준비 완료*/
        public void init() {
            createKernels();
            declareMeta();
            compileKernel();
        }

        private void declareContext()
        {
            ComputeContextPropertyList properties = new ComputeContextPropertyList(ComputePlatform.Platforms[0]);
            ctx = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);
        }

        /** @brief GPGPU를 위해 DEVICE에 충분한 메모리 공간을 할당하고, HOST의 데이터를 GPGPU로 복사함 */
        private void declareMeta()
        {
            cq = new ComputeCommandQueue(ctx, ctx.Devices[0], ComputeCommandQueueFlags.None);
            prog = new ComputeProgram(ctx, kernels);
            
            try
            {
                prog.Build(null, null, null, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                string log = prog.GetBuildLog(ctx.Devices[0]);
                Console.Write(log);
                throw ex;
            }
            gpu_lx = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, new int[1] { data.size[0] });
            gpu_ly = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, new int[1] { data.size[1] });
            gpu_u0 = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, new double[1] { data.u0 });
            gpu_number = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, new int[1]);
            gpu_number9 = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, new int[1]);
            gpu_omega = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, new double[1] { (1d / data.tau) });
            gpu_inLocal = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.inletID);
            gpu_map = new ComputeBuffer<int>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.map);
            gpu_u = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.ux);
            gpu_v = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.uy);
            gpu_density = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.density);
            gpu_fin = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.fin);
            gpu_fout = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.fout);
            gpu_criteria = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.diff);
            gpu_up = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.up);
            gpu_strain = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.strain);
            gpu_dynamic = new ComputeBuffer<double>(ctx, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, data.dynamic);

            worker = new long[] { (long)(data.size[0] * data.size[1]) };
            worker9 = new long[] { (long)(data.size[0] * data.size[1] * 9) };
        }

        /** @brief kernel code를 컴파일 하는 메소드 */
        private void compileKernel()
        {
            int gridSize = data.size[0] * data.size[1];

            /* 병렬 연산을 위해 한번에 연산을 수행할 thread의 숫자를 설정함 */
            cq.Write<int>(gpu_number, new int[] { gridSize }, null);
            cq.Write<int>(gpu_number9, new int[] { gridSize * 9 }, null);

            /* 미리 설정한 kernel code를 컴파일한다 */
            kernelCollision = prog.CreateKernel("collision");
            kernelStream = prog.CreateKernel("stream");
            kernelSwap = prog.CreateKernel("swap");
            kernelBounceback = prog.CreateKernel("bounceback");
            kernelBC = prog.CreateKernel("boundary");
            kernelMacro = prog.CreateKernel("macroscopic");
            KernelMaxima = prog.CreateKernel("maxima");

            /* kernelCollision에 들어갈 parameter를 설정함 */
            kernelCollision.SetMemoryArgument(0, gpu_map);
            kernelCollision.SetMemoryArgument(1, gpu_fin);
            kernelCollision.SetMemoryArgument(2, gpu_u);
            kernelCollision.SetMemoryArgument(3, gpu_v);
            kernelCollision.SetMemoryArgument(4, gpu_omega);
            kernelCollision.SetMemoryArgument(5, gpu_density);
            kernelCollision.SetMemoryArgument(6, gpu_fout);
            kernelCollision.SetMemoryArgument(7, gpu_number9);

            /* kernelStream에 들어갈 parameter를 설정함 */
            kernelStream.SetMemoryArgument(0, gpu_fout);
            kernelStream.SetMemoryArgument(1, gpu_fin);
            kernelStream.SetMemoryArgument(2, gpu_lx);
            kernelStream.SetMemoryArgument(3, gpu_ly);
            kernelStream.SetMemoryArgument(4, gpu_map);
            kernelStream.SetMemoryArgument(5, gpu_number9);

            /* kernelSwap에 들어갈 parameter를 설정함 */
            kernelSwap.SetMemoryArgument(0, gpu_fin);
            kernelSwap.SetMemoryArgument(1, gpu_fout);
            kernelSwap.SetMemoryArgument(2, gpu_map);
            kernelSwap.SetMemoryArgument(3, gpu_number9);

            /* kernelBounceback에 들어갈 parameter를 설정함 */
            kernelBounceback.SetMemoryArgument(0, gpu_fout);
            kernelBounceback.SetMemoryArgument(1, gpu_fin);
            kernelBounceback.SetMemoryArgument(2, gpu_lx);
            kernelBounceback.SetMemoryArgument(3, gpu_ly);
            kernelBounceback.SetMemoryArgument(4, gpu_map);
            kernelBounceback.SetMemoryArgument(5, gpu_number9);

            /* kernelBC에 들어갈 parameter를 설정함 */
            kernelBC.SetMemoryArgument(0, gpu_fin);
            kernelBC.SetMemoryArgument(1, gpu_lx);
            kernelBC.SetMemoryArgument(2, gpu_inLocal);
            kernelBC.SetMemoryArgument(3, gpu_map);
            kernelBC.SetMemoryArgument(4, gpu_u0);
            kernelBC.SetMemoryArgument(5, gpu_number);

            /* kernelMacro에 들어갈 parameter를 설정함 */
            kernelMacro.SetMemoryArgument(0, gpu_fin);
            kernelMacro.SetMemoryArgument(1, gpu_density);
            kernelMacro.SetMemoryArgument(2, gpu_u);
            kernelMacro.SetMemoryArgument(3, gpu_v);
            kernelMacro.SetMemoryArgument(4, gpu_strain);
            kernelMacro.SetMemoryArgument(5, gpu_dynamic);
            kernelMacro.SetMemoryArgument(6, gpu_map);
            kernelMacro.SetMemoryArgument(7, gpu_number);

            /* kernelMaxima에 들어갈 parameter를 설정함 */
            KernelMaxima.SetMemoryArgument(0, gpu_map);
            KernelMaxima.SetMemoryArgument(1, gpu_up);
            KernelMaxima.SetMemoryArgument(2, gpu_u);
            KernelMaxima.SetMemoryArgument(3, gpu_criteria);
            KernelMaxima.SetMemoryArgument(4, gpu_number);
        }

        /** @brief kernel code를 string kernels에 저장하는 메소드\n
        * CPU code를 for loop unrolling으로 병렬화함*/
        public void createKernels()
        {
            // set the kernels
            kernels = @"__kernel void collision(
                        global int* map, 
                        global double* fin, 
                        global double* u, 
                        global double* v, 
                        global double* omega,
                        global double* density, 
                        global double* fout, 
                        global int* N
) 
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
        int grid = tid / 9;

        double ex[] = {0, 1, 0, -1, 0, 1, -1, -1, 1};
        double ey[] = {0, 0, 1, 0, -1, 1, 1, -1, -1};
        double weight[] = {4.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/36.0, 1.0/36.0, 1.0/36.0, 1.0/36.0};
        
        if(map[grid] != 1) {
            double eu, usr;
            int n = tid % 9;
            double feq = 0;
            
            eu = u[grid] * ex[n] + v[grid] * ey[n];
            usr = u[grid] * u[grid] + v[grid] * v[grid];
            feq = weight[n] * density[grid] * (1.0 + 3.0*eu + 4.5*eu*eu - 1.5*usr);
            fout[tid] = fin[tid] - omega[0] * (fin[tid] - feq);
        }
    }
}

__kernel void stream(
                    global double* fout, 
                    global double* fin, 
                    global int* lx, 
                    global int* ly, 
                    global int* map, 
                    global int* N
) {
    int tid = get_global_id(0);
    if(tid < N[0]) {
        int grid = tid / 9;
        int n = tid % 9;
        int gx, gy, dx, dy, nx, ny, dt;

        int ex[] = {0, 1, 0, -1, 0, 1, -1, -1, 1};
        int ey[] = {0, 0, 1, 0, -1, 1, 1, -1, -1};

        nx = lx[0];
        ny = ly[0];

        if(map[grid] != 1) {

            gx = grid % nx + ex[n];
            gy = grid / nx + ey[n];

            dx = gx % nx + nx;
            dy = gy % ny + ny;

            dx = dx % nx;
            dy = dy % ny;

            dt = 9*(dx + nx * dy) + n;

            fin[dt] = fout[tid];
        }
    }
}

__kernel void swap(
                    global double* fin, 
                    global double* fout, 
                    global int* map, 
                    global int* N
)
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
        int grid = tid / 9;

        if(map[grid] == 1) {
            int sindex[] = {0, 2, 2,-2, -2, 2, 2, -2, -2};
            int n = tid % 9;
            fout[tid + sindex[n]] = fin[tid];
        }
    }
}

__kernel void bounceback(
                        global double* fout, 
                        global double* fin, 
                        global int* nx, 
                        global int* ny,
                        global int* map, 
                        global int* N
)
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
        int lx, ly, grid, n;
        lx = nx[0];
        ly = ny[0];
        grid = tid / 9;
        n = tid % 9;
        
        if (map[grid] == 1) {
            int ex[] = {0, 1, 0, -1, 0, 1, -1, -1, 1};
            int ey[] = {0, 0, 1, 0, -1, 1, 1, -1, -1};

            int gx, gy, dx, dy;
            gx = grid % lx + ex[n];
            gy = grid / lx + ey[n];

            dx = (gx % lx + lx)%lx;
            dy = (gy % ly + ly)%ly;

            fin[9 * (dx + lx * dy) + n] = fout[tid];
        }
    }
}

__kernel void boundary(
                        global double* fin, 
                        global int* nx, 
                        global int* inletID,
                        global int* map, 
                        global double* u0, 
                        global int* N
)
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
        int lx = nx[0];
        double a = (double)(inletID[0]/lx);
        double b = (double)(inletID[1]/lx);
        double c = (double)(inletID[2]/lx);
        double d = (double)(inletID[3]/lx);
        double j = tid / lx;
        double uu = u0[0];
        double ux = 0;
        double uy = 0;
        double density = 0;
        
        if (map[tid] == 2) {
            ux = -4.0 * uu / ((a - b)*(a - b)) * (j - a) * (j - b);
            density = (fin[9 * tid + 0] + fin[9 * tid + 2] + fin[9 * tid + 4] + 2.0 * (fin[9 * tid + 3] + fin[9 * tid + 7] + fin[9 * tid + 6])) / (1.0 - ux);
            
            fin[9 * tid + 1] = fin[9 * tid + 3] + density * ux * (2.0 / 3.0);
            fin[9 * tid + 5] = fin[9 * tid + 7] - (fin[9 * tid + 2] - fin[9 * tid + 4]) / 2.0 + density * ux / 6.0;
            fin[9 * tid + 8] = fin[9 * tid + 6] + (fin[9 * tid + 2] - fin[9 * tid + 4]) / 2.0 + density * ux / 6.0;
        }

        if (map[tid] == 3) {
            // t-shaped
            ux = 4.0 * uu / ((c - d)*(c - d)) * (j - c) * (j - d);
            density = (fin[9 * tid + 0] + fin[9 * tid + 2] + fin[9 * tid + 4] + 2.0 * (fin[9 * tid + 1] + fin[9 * tid + 5] + fin[9 * tid + 8])) / (1.0 + ux);
            
            fin[9 * tid + 3] = fin[9 * tid + 1] - density * ux * (2.0 / 3.0);
            fin[9 * tid + 7] = fin[9 * tid + 5] + (fin[9 * tid + 2] - fin[9 * tid + 4]) / 2.0 - density * ux / 6.0;
            fin[9 * tid + 6] = fin[9 * tid + 8] - (fin[9 * tid + 2] - fin[9 * tid + 4]) / 2.0 - density * ux / 6.0;

            //fin[9 * tid + 3] = fin[9 * (tid - 1) + 3];
            //fin[9 * tid + 7] = fin[9 * (tid - 1) + 7];
            //fin[9 * tid + 6] = fin[9 * (tid - 1) + 6];       
        }

        if (map[tid] == 4) {
            fin[9 * tid + 2] = fin[2 + 9 * (tid + lx)];
            fin[9 * tid + 5] = fin[5 + 9 * (tid + lx)];
            fin[9 * tid + 6] = fin[6 + 9 * (tid + lx)];

            // density = 1.0;
            // ux = 0;
            // uy = 1.0 - (fin[0 + 9 * tid] + fin[1 + 9 * tid] + fin[3 + 9 * tid] + 2.0 * (fin[4 + 9 * tid] + fin[7 + 9 * tid] + fin[8 + 9 * tid])) / density;
            // 
            // fin[2 + 9 * tid] = fin[4 + 9 * tid] + (2.0 / 3.0) * density * uy;
            // fin[5 + 9 * tid] = fin[7 + 9 * tid] + (fin[3 + 9 * tid] - fin[1 + 9 * tid]) / 2.0 + density * uy / 6.0;
            // fin[6 + 9 * tid] = fin[8 + 9 * tid] + (fin[1 + 9 * tid] - fin[3 + 9 * tid]) / 2.0 + density * uy / 6.0;
        }
    }
}

__kernel void macroscopic(
                            global double* fin, 
                            global double* density, 
                            global double* u, 
                            global double* v,
                            global double* strain,
                            global double* dynamic,
                            global int* map,
                            global int* N)
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
        double ex[] = {0, 1.0, 0, -1.0, 0, 1.0, -1.0, -1.0, 1.0};
        double ey[] = {0, 0, 1.0, 0, -1.0, 1.0, 1.0, -1.0, -1.0};
        double weight[] = {4.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/36.0, 1.0/36.0, 1.0/36.0, 1.0/36.0};
        double sxx, sxy, syx, syy, feq, eu, usqr;
        double buffer, buffer_u, buffer_v;

        buffer = 0;
        buffer_u = 0;
        buffer_v = 0;
        sxx = 0;
        sxy = 0;
        syx = 0;
        syy = 0;
        
        if(map[tid] != 1) {
            for(int n = 0 ; n < 9 ; n++) {
	            buffer += fin[9 * tid + n];
                buffer_u += ex[n] * fin[9 * tid + n];
                buffer_v += ey[n] * fin[9 * tid + n];
            }

            density[tid] = buffer;
            u[tid] = buffer_u / buffer;
            v[tid] = buffer_v / buffer;

            usqr = u[tid]*u[tid] + v[tid]*v[tid];
            for(int n = 0; n < 9; n++) {
                eu = ex[n] * u[tid] + ey[n] * v[tid];
                feq = weight[n] * density[tid] * (1.0 + 3.0 * eu + 4.5 * eu * eu - 1.5 * usqr);

                sxx += ex[n] * ex[n] * (fin[n + 9 * tid] - feq);
                syx += ey[n] * ex[n] * (fin[n + 9 * tid] - feq);
                sxy += ex[n] * ey[n] * (fin[n + 9 * tid] - feq);
                syy += ey[n] * ey[n] * (fin[n + 9 * tid] - feq);
            }
            strain[tid] = sqrt(sxx*sxx + sxy*sxy + syx*syx + syy*syy);
            dynamic[tid] = density[tid] * usqr / 2.0;
        }
    }
}

__kernel void maxima(
                            global int* map,
                            global double* u0,
                            global double* u1,
                            global double* output,
                            global int* N)
{
    int tid = get_global_id(0);
    if(tid < N[0]) {
         if(map[tid] == 0) {
            output[tid] = fabs(u1[tid] - u0[tid]);
         }
    }
}
";
        }

        /** @brief GPGPU 연산을 수행하는 메소드\n
        * cq.Execute를 이용하여 미리 생성한 kernel을 수행하고 결과 값을 HOST로 전송함*/
        public void run()
        {
            // actuators
            cq.Execute(kernelCollision, null, worker9, null, null);
            cq.Execute(kernelStream, null, worker9, null, null);
            cq.Execute(kernelSwap, null, worker9, null, null);
            cq.Execute(kernelBounceback, null, worker9, null, null);
            cq.Execute(kernelBC, null, worker, null, null);
            cq.Execute(kernelMacro, null, worker, null, null);

            data.up = data.ux.Clone() as double[];
            data.ux = cq.Read<double>(gpu_u, null);
            data.uy = cq.Read<double>(gpu_v, null);
            data.density = cq.Read<double>(gpu_density, null);
            data.strain = cq.Read<double>(gpu_strain, null);
            data.dynamic = cq.Read<double>(gpu_dynamic, null);
            cq.Finish();
        }

        /** @brief kernelMaxima를 수행하여 iteration 간의 velocity 값의 차이를 계산함 */
        private void test()
        {
            cq.Write<double>(gpu_up, data.up, null);
            cq.Write<double>(gpu_u, data.ux, null);
            cq.Execute(KernelMaxima, null, worker, null, null);
            cq.Finish();

            data.diff = cq.Read<double>(gpu_criteria, null);
        }

        /** @brief kernelMaxima를 수행 결과로 Steady-state check를 수행함
        * @return Steady-state에 도달했다면 false를 반환함 */
        public bool getError(bool check)
        {
            double maxima = 0;

            if (check)
            {
                test();
                maxima = data.diff.Max();
                if (maxima < data.criteria)
                {
                    data.diff[0] = maxima;
                    data.sb.AppendLine(maxima.ToString());
                    return false;
                }
                else
                {
                    data.diff[0] = maxima;
                    data.sb.AppendLine(maxima.ToString());
                    return true;
                }
            }
            return true;
        }

        /** @brief 시뮬레이션 변수를 초기화 하는 메소드 */
        public void resetAll()
        {
            data.init();

            cq.Write<double>(gpu_u, data.ux, null);
            cq.Write<double>(gpu_v, data.uy, null);
            cq.Write<double>(gpu_density, data.density, null);
            cq.Write<double>(gpu_strain, data.strain, null);
            cq.Write<double>(gpu_dynamic, data.dynamic, null);
            cq.Write<double>(gpu_fin, data.fin, null);
            cq.Write<int>(gpu_map, data.map, null);
            cq.Finish();
        }
    }
}

