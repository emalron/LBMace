using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LBMace
{
    /**
    * @brief 시뮬레이션 결과를 후처리하는 클래스 \n
    * 후처리는 Paraview 포맷인 VTK 파일에 데이터를 저장하는 과정과 Geometry 정보를 Bitmap 파일에 저장하는 과정이 있다.
    */
    class Postprocess
    {
        /** @brief 시뮬레이션 데이터를 얻기 위해 data 클래스를 불러옴 */
        Data data;
        /** @brief iteration 값 */
        public int iter;
        /** @brief 후처리 결과가 저장될 경로 */
        string filepath;
        /** @brief 후처리 결과가 저장될 파일의 prefix */
        string filename;
        /** @brief prefix와 iteration #를 포함한 파일명 */
        string name;

        /** @brief filepath를 읽기 전용으로 설정함 */
        public string Filepath
        {
            get
            {
                return filepath;
            }
        }

        public Postprocess()
        {
            iter = 0;
        }

        /** @brief 후처리 결과가 저장될 메타 정보를 입력하는 메소드
        * @param path 후처리 결과가 저장될 경로
        * @param name 후처리 결과 파일의 prefix
        */
        public void setMeta(string path, string name)
        {
            /* 시뮬레이션 데이터를 읽기 위해 data에 data의 객체 정보를 얻어옴 */
            data = Data.get();

            iter = 0;
            filepath = path;
            filename = name;
        }

        /** @brief 후처리 결과 파일의 이름을 확정하는 메소드
        * @param mode 후처리 결과 파일의 확장자로 vtk와 bmp 중 하나의 값을 가진다.
        * @return 파일 경로 + 파일 이름 + 파일 확장자를 반환함
        */
        private string nameFiles(string ext)
        {
            string output = String.Format(@"{0}\{1}{2}.{3}", filepath, filename, iter, ext);
            return output;
        }

        /** @brief 후처리 결과 파일을 vtk 포맷으로 저장하는 메소드\n
        * 이하 내용은 vtk 포맷에 따라 density, velocity vector, strain rate tensor 등이 저장된다.
        */
        public void saveFiles()
        {
            StringBuilder post = new StringBuilder();

            post.Append("# vtk DataFile Version 3.0");
            post.AppendLine();
            post.Append("fluid_state");
            post.AppendLine();
            post.Append("ASCII");
            post.AppendLine();
            post.Append("DATASET STRUCTURED_POINTS");
            post.AppendLine();
            post.Append("DIMENSIONS " + data.size[0].ToString() + " " + data.size[1].ToString() + " 1");
            post.AppendLine();

            post.Append("ORIGIN 0 0 0");
            post.AppendLine();
            post.Append("SPACING 1 1 1");
            post.AppendLine();

            post.Append("POINT_DATA " + (data.size[0] * data.size[1]).ToString());
            post.AppendLine();
            post.Append("SCALARS density double 1");
            post.AppendLine();
            post.Append("LOOKUP_TABLE default");
            post.AppendLine();

            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.Append(data.density[index].ToString());
                post.AppendLine();
            }

            post.Append("VECTORS velocity_vector double");
            post.AppendLine();
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.Append(data.ux[index].ToString() + " " + data.uy[index].ToString() + " 0");
                post.AppendLine();
            }

            post.Append("SCALARS strain double 1");
            post.AppendLine();
            post.Append("LOOKUP_TABLE sxx");
            post.AppendLine();
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.Append(data.strain[index]);
                post.AppendLine();
            }

            post.Append("SCALARS dynamic double 1");
            post.AppendLine();
            post.Append("LOOKUP_TABLE dyn");
            post.AppendLine();
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.Append(data.dynamic[index]);
                post.AppendLine();
            }

            name = nameFiles("vtk");
            System.IO.File.WriteAllText(name, post.ToString());
        }

        /** @brief 후처리 결과 Geometry 정보를 Bitmap 포맷으로 저장하는 메소드
        * fluid(white), solid(black), inlet(red and blue), outlet(green)으로 bitmap에 저장된다.
        */
        public void saveImages()
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

            name = nameFiles("bmp");
            buffer.Save(name, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
