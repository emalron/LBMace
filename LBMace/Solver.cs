﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBMace
{
    /** @brief Simulation의 계산이 이루어지는 클래스\n
    * CPU를 이용한 연산과 Shape optimization 로직이 포함되어 있다.\n
    * Shape optimization은 그 interface의 숫자가 제한적이므로 GPGPU를 통한 가속 효과를 받기가 어려워 CPU로 연산을 수행한다.
    */
    class Solver
    {
        Data data;

        private double[] weight, ex, ey;
        private int[] opp;

        public Solver()
        {
            /* D2Q9 Scheme의 weighting factor로 각 distribution function이 갖는 고유한 값이다. */
            weight = new double[9] { 4d / 9d, 1d / 9d, 1d / 9d, 1d / 9d, 1d / 9d, 1d / 36d, 1d / 36d, 1d / 36d, 1d / 36d };

            /* D2Q9 Scheme의 각 distribution function이 갖는 unit vector 중 x축 값이다 */
            ex = new double[9] { 0, 1d, 0, -1d, 0, 1d, -1d, -1d, 1d };

            /* D2Q9 Scheme의 각 distribution function이 갖는 unit vector 중 y축 값이다 */
            ey = new double[9] { 0, 0, 1d, 0, -1d, 1d, 1d, -1d, -1d };

            /* D2Q9 Scheme의 각 distribution function과 반대 방향인 distribution function의 번호 */
            opp = new int[9] { 0, 3, 4, 1, 2, 7, 8, 5, 6 };

            /* 시뮬레이션 데이터에 접근 하기 위해 싱글턴 data 클래스를 호출함 */
            data = Data.get();
        }

        /** @brief Collision step을 계산하는 클래스\n
        * fin 배열의 값에 대한 collision의 결과가 fout 배열에 저장된다.
        */
        public void collision()
        {
            double eu, eusqr, usqr, feq;
            int MAP_SIZE = data.size[0] * data.size[1];

            for (int index = 0; index < MAP_SIZE; index++)
            {
                if (data.map[index] != 1)
                {
                    usqr = Math.Pow(data.ux[index], 2) + Math.Pow(data.uy[index], 2);
                    for (int n = 0; n < 9; n++)
                    {
                        eu = ex[n] * data.ux[index] + ey[n] * data.uy[index];
                        eusqr = eu * eu;
                        feq = weight[n] * data.density[index] * (1d + 3d * eu + 4.5d * eusqr - 1.5d * usqr);

                        data.fout[n + 9 * index] = data.fin[n + 9 * index] - (1d/data.tau) * (data.fin[n + 9 * index] - feq);
                    }
                }
            }
        }

        /** @brief 인접한 lattice로 distribution function을 이동시키는 클래스\n
        * @param input streaming 전의 distribution function 배열
        * @param output streaming 과정으로 이동한 distribution function의 배열
        * @param mode 값에 의해 streaming/bounce back이 바뀌며 streaming은 fluid의 distribution function이 이동하고, bounce back 과정에선 wall의 distribution function이 이동한다.
        */
        public void streaming(double[] input, double[] output, int mode)
        {
            int ax, ay, bx, by;
            int MAP_SIZE = data.size[0] * data.size[1];

            for (int index = 0; index < MAP_SIZE; index++)
            {
                int i = index % data.size[0];
                int j = index / data.size[0];

                if (isWall(mode, index))
                {
                    for (int n = 0; n < 9; n++)
                    {
                        ax = i + (int)ex[n];
                        ay = j + (int)ey[n];

                        bx = ((ax % data.size[0]) + data.size[0]) % data.size[0];
                        by = ((ay % data.size[1]) + data.size[1]) % data.size[1];

                        output[n + 9 * (bx + data.size[0] * by)] = input[n + 9 * index];
                    }
                }
            }
        }

        /** @brief 지정된 index 값의 lattice가 wall인지 solid인지 판별함
        * @param mode map 배열의 값이 input으로 제공된다
        * @return 성공 여부
        */
        private bool isWall(int mode, int index)
        {
            bool output = true;

            switch (mode)
            {
                case 0:
                    if (data.map[index] != 1)
                    {
                        output = true;
                    }
                    else
                    {
                        output = false;
                    }
                    break;

                case 1:
                    if (data.map[index] == 1)
                    {
                        output = true;
                    }
                    else
                    {
                        output = false;
                    }
                    break;
            }
            return output;
        }

        /** @brief wall에 대하여 full-way bounce back scheme을 수행함
        * wall lattice가 가진 distribution function의 방향을 전부 반대로 바꾸고\n
        * 바뀐 distribution function을 인접한 lattice로 streaming 한다.
        */
        public void bounceback()
        {
            int MAP_SIZE = data.size[0] * data.size[1];

            for (int index = 0; index < MAP_SIZE; index++)
            {
                if (data.map[index] == 1)
                {
                    for (int n = 0; n < 9; n++)
                    {
                        data.fout[opp[n] + 9 * index] = data.fin[n + 9 * index];
                    }
                }
            }

            streaming(data.fout, data.fin, 1);
        }

        /** @brief 경계조건 계산하는 메소드\n
        * 이 경우 경계조건은 inlet과 outlet의 zou and he boundary condition이거나 interpolated boundary condition이다.*/
        public void boundary()
        {
            int MAP_SIZE = data.size[0] * data.size[1];

            for (int index = 0; index < MAP_SIZE; index++)
            {
                switch (data.map[index])
                {
                    case 2://inlet left
                        inletLeft(index);
                        break;

                    case 3://inlet right
                        inletRight(index);
                        break;

                    case 4://outlet bottom
                        outletOpen(index);
                        break;
                }
            }
        }

        /** @brief 좌측(Red) inlet의 zou and he constant velocity condition을 계산함\n */
        private void inletLeft(int index)
        {
            int index9 = 9 * index;

            data.ux[index] = (-4.0d * 1.5d * data.u0 / Math.Pow((data.inletID[0]/data.size[0] - data.inletID[1]/data.size[0]), 2)) * (index / data.size[0] - data.inletID[0]/data.size[0]) * (index / data.size[0] - data.inletID[1]/data.size[0]);
            data.uy[index] = 0;
            
            data.density[index] = (data.fin[0 + index9] + data.fin[2 + index9] + data.fin[4 + index9] + 2d * (data.fin[3 + index9] + data.fin[6 + index9] + data.fin[7 + index9])) / (1 - data.ux[index]);
            
            data.fin[1 + index9] = data.fin[3 + index9] + (2d / 3d) * data.density[index] * data.ux[index];
            data.fin[5 + index9] = data.fin[7 + index9] - (data.fin[2 + index9] - data.fin[4 + index9]) / 2d + (1d / 6d) * data.density[index] * data.ux[index];
            data.fin[8 + index9] = data.fin[6 + index9] + (data.fin[2 + index9] - data.fin[4 + index9]) / 2d + (1d / 6d) * data.density[index] * data.ux[index];

            // Lid-Driven Cavity
            //
            //data.ux[index] = data.u0;
            //data.uy[index] = 0;
            //
            //data.density[index] = (data.fin[0 + index9] + data.fin[1 + index9] + data.fin[3 + index9] + 2.0d * (data.fin[2 + index9] + data.fin[5 + index9] + data.fin[6 + index9]));
            //data.fin[4 + index9] = data.fin[2 + index9];
            //data.fin[7 + index9] = data.fin[5 + index9] + 0.5d * (data.fin[1 + index9] - data.fin[3 + index9]) - 0.5d * data.density[index] * data.ux[index];
            //data.fin[8 + index9] = data.fin[6 + index9] - 0.5d * (data.fin[1 + index9] - data.fin[3 + index9]) + 0.5d * data.density[index] * data.ux[index];
        }

        /** @brief 우측(Blue) inlet의 zou and he constant velocity condition을 계산함\n */
        private void inletRight(int index)
        {
            int index9 = 9 * index;

            data.ux[index] = (4.0d * 1.5d * data.u0 / Math.Pow((data.inletID[2] / data.size[0] - data.inletID[3] / data.size[0]), 2)) * (index / data.size[0] - data.inletID[2] / data.size[0]) * (index / data.size[0] - data.inletID[3] / data.size[0]);
            data.uy[index] = 0;

            data.density[index] = (data.fin[0 + index9] + data.fin[2 + index9] + data.fin[4 + index9] + 2d * (data.fin[1 + index9] + data.fin[5 + index9] + data.fin[8 + index9])) / (1 + data.ux[index]);

            data.fin[3 + index9] = data.fin[1 + index9] - (2d / 3d) * data.density[index] * data.ux[index];
            data.fin[7 + index9] = data.fin[5 + index9] + (data.fin[2 + index9] - data.fin[4 + index9]) / 2d - (1d / 6d) * data.density[index] * data.ux[index];
            data.fin[6 + index9] = data.fin[8 + index9] - (data.fin[2 + index9] - data.fin[4 + index9]) / 2d - (1d / 6d) * data.density[index] * data.ux[index];
        }

        /** @brief outlet의 Open-ended boundary condition을 계산함\n
        * interpolated condition을 이용하여 계산함 */
        private void outletOpen(int index)
        {
            int F_SIZE = 9;
            int index9 = F_SIZE * index;
            

            data.fin[2 + index9] = data.fin[2 + F_SIZE * (index + data.size[0] * 1)];
            data.fin[5 + index9] = data.fin[5 + F_SIZE * (index + data.size[0] * 1)];
            data.fin[6 + index9] = data.fin[6 + F_SIZE * (index + data.size[0] * 1)];
        }

        /** @brief distribution function의 moment를 계산하는 메소드\n
        * 이 메소드에서 density, ux, uy, strain의 값이 계산된다 */
        public void macroscopic()
        {
            int MAP_SIZE, F_SIZE;

            MAP_SIZE = data.size[0] * data.size[1];
            F_SIZE = 9;

            double sxx, syx, sxy, syy;
            double eu, feq, usqr;

            for(int index = 0; index < MAP_SIZE;index++)
            {
                data.up[index] = data.ux[index];
                data.vp[index] = data.uy[index];
            }

            for(int index = 0; index < MAP_SIZE;index++)
            {
                if (data.map[index] != 1)
                {
                    data.density[index] = 0;
                    data.ux[index] = 0;
                    data.uy[index] = 0;
                    sxx = 0;
                    syx = 0;
                    sxy = 0;
                    syy = 0;

                    for (int n = 0; n < F_SIZE; n++)
                    {
                        data.density[index] += data.fin[n + F_SIZE * index];
                        data.ux[index] += ex[n] * data.fin[n + F_SIZE * index];
                        data.uy[index] += ey[n] * data.fin[n + F_SIZE * index];
                    }

                    data.ux[index] /= data.density[index];
                    data.uy[index] /= data.density[index];

                    usqr = Math.Pow(data.ux[index], 2) + Math.Pow(data.uy[index], 2);
                    for (int n = 0; n < F_SIZE; n++)
                    {
                        eu = ex[n] * data.ux[index] + ey[n] * data.uy[index];
                        feq = weight[n] * data.density[index] * (1d + 3d * eu + 4.5d * eu * eu - 1.5d * usqr);

                        sxx += ex[n] * ex[n] * (data.fin[n + F_SIZE * index] - feq);
                        sxy += ex[n] * ey[n] * (data.fin[n + F_SIZE * index] - feq);
                        syx += ey[n] * ex[n] * (data.fin[n + F_SIZE * index] - feq);
                        syy += ey[n] * ey[n] * (data.fin[n + F_SIZE * index] - feq);
                    }
                    data.strain[index] = Math.Sqrt(Math.Pow(sxx, 2) + Math.Pow(sxy, 2) + Math.Pow(syx, 2) + Math.Pow(syy, 2));
                }
            }
        }

        /** @brief 시뮬레이션의 residue를 계산하는 메소드\n
        * @param mod getError의 on/off 기능
        * @return residue가 기준 값 이하라면 false를 반환한다 = steady-state에 도달 함*/
        public bool getError(bool mod)
        {
            double maxima = 0;
            double umax = data.ux.Max();
            double vmax = data.uy.Max();
            double realMax = 0;
            int MAP_SIZE = data.size[0] * data.size[1];

            double[] crit = new double[2];
            crit[0] = 0;
            crit[1] = 0;

            if (mod) {
                for (int n = 0; n < MAP_SIZE; n++)
                {
                    crit[0] = Math.Abs(data.ux[n] - data.up[n]) / umax;
                    crit[1] = Math.Abs(data.uy[n] - data.vp[n]) / vmax;

                    realMax = crit.Max();

                    if (realMax > maxima) {
                        maxima = realMax;
                        data.diff[0] = maxima;
                    }
                }
                if (maxima < data.criteria)
                {
                    data.diff[0] = maxima;
                    return false;
                }
            }

            return true;
        }

        /** @brief Shape Optimziation의 코어 로직\n
        * interface를  먼저 탐색하고, shape optimization 로직에 따라 Geometry를 수정한다. */
        public void optimize()
        {
            Dictionary<int, double> interfaces = new Dictionary<int, double>();
            interfaces = interfaceSearch();
            controlWall(interfaces);
        }
        
        /** @brief Geometry에서 경계선을 탐색하는 메소드\n
        * 지정한 범위의 design domain 내의 solid/fluid interface가 갖는 strain rate tensor의 값을 앞서 선언한 dictionary minDic, MaxDic에 저장함 */
        private Dictionary<int, double> interfaceSearch()
        {

            // find all flud tiles
            var fluidTiles =
                from index in Enumerable.Range(0, data.map.Length)
                where data.map[index] == 0
                select index;

            // select fluid tiles which have at least an adjecent solid tile among the fluid tiles
            var interfaceTiles =
                from index in fluidTiles
                from n in Enumerable.Range(0, 9)
                let j = (int)(index / data.size[0]) + (int)(ey[n])
                let i = (int)(index % data.size[0]) + (int)(ex[n])
                let boundaryCheck = i > 1 && (i < data.size[0] - 2) && j > 1 && (j < data.size[1] - 2)
                where boundaryCheck == true
                where data.map[i + j * data.size[0]] == 1
                select index;

            var result = interfaceTiles.Distinct().ToDictionary(o => o, o => data.strain[o]);

            return result;
        }

        /** @brief Shape optimization 로직에 의해 Geometry를 변경하는 메소드\n
        * Dictionary의 값을 정렬하여 최대/최소 strain rate tensor 값과 해당 lattice의 ID를 얻고, Exchange rate(data.xrate) 만큼 solid/fluid를 서로 교환한다 */
        private void controlWall(Dictionary<int, double> interfaces)
        {
            // interface tiles을 strain 값으로 내림차순함.
            var rank = interfaces.OrderByDescending(x => x.Value);
            int num = interfaces.Count;
            int rate = (int)(num * data.xrate / 100d);

            // 상위 strain 값을 가진 타일 주변의 wall tile을 찾음, 중복 제거 및 exchange rate 만큼 획득
            var toFluid =
                from tile in rank
                from n in Enumerable.Range(0, 4)
                let j = (int)(tile.Key / data.size[0]) + (int)(ey[n])
                let i = (int)(tile.Key % data.size[0]) + (int)(ex[n])
                let boundaryCheck = i > 1 && (i < data.size[0] - 2) && j > 1 && (j < data.size[1] - 2)
                where boundaryCheck == true
                where data.map[i + j * data.size[0]] == 1
                select new { Key = tile.Key, Value = i + j * data.size[0] };

            //맵 변화
            foreach (var item in toFluid.Take(rate))
            {
                data.map[item.Value] = 0;
            }

            // interface strain 값을 기준으로 오름차순으로 정렬함
            var revRank =
                from tile in interfaces
                orderby tile.Value
                select tile;

            // 그중 하위 fluid 타일을 exchange rate 만큼 확보함
            var toSolid = revRank.Take(rate);

            // 확보된 fluid 타일을 solid 타일로 변경함
            foreach(var item in toSolid)
            {
                data.map[item.Key] = 1;
            }
        }

        /** @brief Shape optimization 과정이 끝나고 모든 시뮬레이션 변수의 값을 초기화 하는 함수 */
        public void resetAll()
        {
            data.init();
        }
    }
}
