using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using Point = OpenCvSharp.Point;

namespace TestVisionMaster
{
    public static class Tool
    {
        public static string ToIntStr(bool value) => value ? "1" : "0";
        /// <summary>
        /// Mat转图片
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Bitmap ConvertMatToBitmap(Mat mat, PixelFormat pixelFormat)
        {
            return ConvertByteArrayToBitmap(mat.ToBytes(), pixelFormat);
        }
        public static Mat CreatRotaRec(Mat image, Point2f center, Size2f size,float angle)
        {
            RotatedRect rotatedRect = new RotatedRect(center, size, angle);

            // 获取旋转矩形的四个顶点
            Point2f[] points = rotatedRect.Points();

            // 将 Point2f 数组转换为 Point 数组
            Point[] intPoints = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                intPoints[i] = new Point((int)points[i].X, (int)points[i].Y);
            }

            // 绘制旋转矩形
            Cv2.Polylines(image, new[] { intPoints }, true, Scalar.Red, 2);
            return image;
        }
        /// <summary>
        /// 字节数组转图片
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Bitmap ConvertByteArrayToBitmap(byte[] bytes, PixelFormat pixelFormat)
        {
            if (pixelFormat != PixelFormat.Format8bppIndexed && pixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new NotSupportedException("不支持指定格式");
            }

            using (var ms = new MemoryStream(bytes))
            {
                var bitmap = new Bitmap(System.Drawing.Image.FromStream(ms));
                if (bitmap.PixelFormat != pixelFormat)
                {
                    bitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), pixelFormat);
                }

                return bitmap;
            }
        }
        /// <summary>
        /// 字节数组转Mat
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Mat ConvertByteArrayToMat(byte[] bytes)
        {
            Mat mat = Cv2.ImDecode(bytes, ImreadModes.Color);
            return mat;
        }
    }
}
