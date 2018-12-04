using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LBMace
{
    class Postprocess
    {
        /** @brief 시뮬레이션 데이터를 얻기 위해 data 클래스를 불러옴 */
        Data data;
        
        /** @brief iteration 값 */
        public int iter;
        
        public Postprocess()
        {
            iter = 0;
            data = Data.get();
        }

        private string nameFiles(string ext)
        {
            string filepath = data.savePath;
            string filename = data.saveFileName;

            string output = String.Format(@"{0}\{1}{2}.{3}", filepath, filename, iter, ext);
            return output;
        }

        /** @brief 후처리 결과 파일을 vtk 포맷으로 저장하는 메소드\n
        * 이하 내용은 vtk 포맷에 따라 density, velocity vector, strain rate tensor 등이 저장된다.
        */
        public void saveFiles()
        {
            StringBuilder post = new StringBuilder();

            post.AppendLine("# vtk DataFile Version 3.0");
            post.AppendLine("fluid_state");
            post.AppendLine("ASCII");
            post.AppendLine("DATASET STRUCTURED_POINTS");
            post.AppendLine("DIMENSIONS " + data.size[0].ToString() + " " + data.size[1].ToString() + " 1");
            post.AppendLine("ORIGIN 0 0 0");
            post.AppendLine("SPACING 1 1 1");
            post.AppendLine("POINT_DATA " + (data.size[0] * data.size[1]).ToString());

            post.AppendLine("SCALARS density double 1");
            post.AppendLine("LOOKUP_TABLE default");
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.AppendLine(data.density[index].ToString());
            }

            post.AppendLine("VECTORS velocity_vector double");
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                string ux = data.ux[index].ToString();
                string uy = data.uy[index].ToString();
                string velocities = String.Format("{0} {1} 0.0", ux, uy);

                post.AppendLine(velocities);
            }

            post.AppendLine("SCALARS strain double 1");
            post.AppendLine("LOOKUP_TABLE sxx");
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.AppendLine(data.strain[index].ToString());
            }

            post.AppendLine("SCALARS dynamic double 1");
            post.AppendLine("LOOKUP_TABLE dyn");
            for (int index = 0; index < data.size[0] * data.size[1]; index++)
            {
                post.AppendLine(data.dynamic[index].ToString());
            }

            string name = nameFiles("vtk");
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

            string name = nameFiles("bmp");
            buffer.Save(name, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
