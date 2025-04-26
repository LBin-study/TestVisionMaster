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
using System.Runtime.InteropServices;
using System.Collections;

namespace TestVisionMaster
{
    public static class Tool
    {
        public static string ToIntStr(bool value) => value ? "1" : "0";
        private static ColorPalette? palette;
        /// <summary>
        /// 8位灰度Mat转8位灰度Bitmap
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Bitmap ConvertMatToBitmap(Mat mat,PixelFormat format)
        {
            if (format== PixelFormat.Format8bppIndexed)
            {
                if (palette == null)
                {
                    palette = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    }
                }

                byte[] encodedBytes1;
                Cv2.ImEncode(".bmp", mat, out encodedBytes1);
                // 创建 8 位索引的 Bitmap 对象
                Bitmap bitmap = new Bitmap(mat.Width, mat.Height, PixelFormat.Format8bppIndexed);
                bitmap.Palette = palette;
                // 锁定 Bitmap 数据
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, mat.Width, mat.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                try
                {
                    // 跳过文件头（14 字节）、信息头（40 字节）和调色板（1024 字节）
                    Marshal.Copy(encodedBytes1, 1078, bitmapData.Scan0, encodedBytes1.Length - 1078);
                }
                finally
                {
                    // 解锁 Bitmap 数据
                    bitmap.UnlockBits(bitmapData);
                }
                return bitmap;
            }
            return null;
            
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
    }
}
