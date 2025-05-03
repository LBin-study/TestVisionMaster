using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;
using VisionDesigner.Code2DReader;
using VisionDesigner.ContourPatMatch;
using static TestVisionMaster.TemplateMachingTool;
using System.Threading;

namespace TestVisionMaster
{
    internal class QRCodeTool
    {
        /// <summary>
        /// 支持的图像格式(仅支持8位灰度图像)
        /// </summary>
        private const MVD_PIXEL_FORMAT SupportImageFormat = MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08;
        private readonly C2DCodeReaderTool QRCode;
        public bool CanUse { get; } = false;

        public QRCodeTool()
        {
            try
            {
                QRCode = new C2DCodeReaderTool();
                CanUse = true;
            }
            catch (Exception)
            {
                CanUse=false;
            }
        }
        #region 参数列表
        /// <summary>
        /// 条码数量
        /// </summary>
        public int Loc2DCodeNum
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(Loc2DCodeNum), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 1 || value > 10000) throw new ArgumentException("条码数量必须在1-10000之间");
                QRCode.SetRunParam(nameof(Loc2DCodeNum), value.ToString());
            }
        }
        /// <summary>
        /// 最小码宽范围
        /// </summary>
        public int MinBarSize
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(MinBarSize), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 20 || value > 1000) throw new ArgumentException("最小码宽范围必须在20-1000之间");
                if (value> MaxBarSize) throw new ArgumentException("最小码宽必须大于最大码宽");
                QRCode.SetRunParam(nameof(MinBarSize), value.ToString());
            }
        }
        /// <summary>
        /// 最大码宽范围
        /// </summary>
        public int MaxBarSize
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(MaxBarSize), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 1 || value > 10000) throw new ArgumentException("条码数量必须在1-10000之间");
                if (value < MinBarSize) throw new ArgumentException("最大码宽必须大于最小码宽");
                QRCode.SetRunParam(nameof(MaxBarSize), value.ToString());
            }
        }
        /// <summary>
        /// 镜像模式：镜像模式启用开关，指的是图像X方向镜像，包括3种模式：
        ///- 打开：Open=1
        ///- 关闭：Close=0
        ///- 兼容：Compatible=2
        /// </summary>
        public int MirrorMode
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(MirrorMode), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 2) throw new ArgumentException("镜像指令错误，必须为0-2");
                QRCode.SetRunParam(nameof(MirrorMode), value.ToString());
            }
        }
        /// <summary>
        /// 降采样倍数，1-8
        /// </summary>
        public int SampleLevel
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(SampleLevel), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 1 || value > 8) throw new ArgumentException("降采样倍数必须再1-8之间");
                QRCode.SetRunParam(nameof(SampleLevel), value.ToString());
            }
        }
        /// <summary>
        /// 码极性，分为3种：
        ///- 白底黑码：DarkOnBright=0
        ///- 黑底白码：BrightOnDark=1
        ///- 兼容模式：Both=2
        /// </summary>
        public int Polarity
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(Polarity), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 2) throw new ArgumentException("码极性的指令枚举必须为0-2");
                QRCode.SetRunParam(nameof(Polarity), value.ToString());
            }
        }
        /// <summary>
        /// 连续与离散码标志，分为3种：
        ///- 连续码：Continuous=0
        ///- 离散码：Discrete=1
        ///- 兼容模式：Both=2
        /// </summary>
        public int DiscreteFlag
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(DiscreteFlag), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 2) throw new ArgumentException("连续与离散码标志的指令枚举必须为0-2");
                QRCode.SetRunParam(nameof(DiscreteFlag), value.ToString());
            }
        }
        /// <summary>
        /// QR畸变配置参数：0关闭（默认），1开启。
        /// </summary>
        public int DistortionFlag
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(DistortionFlag), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentException("QR畸变配置参数的指令枚举必须为0-1");
                QRCode.SetRunParam(nameof(DistortionFlag), value.ToString());
            }
        }
        /// <summary>
        /// DM正方形长方形码类型，分为3种：
        ///- 正方形：Square=0
        ///- 长方形：Rect=1
        ///- 兼容模式：Both=2
        /// </summary>
        public int RectangleFlag
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(RectangleFlag), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 2) throw new ArgumentException("DM正方形长方形码类型的指令枚举必须为0-2");
                QRCode.SetRunParam(nameof(RectangleFlag), value.ToString());
            }
        }
        /// <summary>
        /// 算法库运行模式，分为3种：
        ///- 普通模式：NormalMode=0
        ///- 专业模式：ProMode=1
        ///- 极速模式：FastMode=2
        ///正常场景下采用普通模式，专业模式预留给较难识别的二维码，当应用场景简单，可以采用极速模式。极速模式不支持极性兼容。
        /// </summary>
        public int AppMode
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(AppMode), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 2) throw new ArgumentException("算法库运行模式的指令枚举必须为0-2");
                QRCode.SetRunParam(nameof(AppMode), value.ToString());
            }
        }
        /// <summary>
        /// QR码开关
        /// </summary>
        public int CodeQRFlag
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(CodeQRFlag), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentException("QR码开关的指令枚举必须为0-1");
                QRCode.SetRunParam(nameof(CodeQRFlag), value.ToString());
            }
        }

        /// <summary>
        /// DM码开关
        /// </summary>
        public int CodeDMFlag
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(CodeDMFlag), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentException("DM码开关的指令枚举必须为0-1");
                QRCode.SetRunParam(nameof(CodeDMFlag), value.ToString());
            }
        }
        /// <summary>
        /// DM码开关
        /// </summary>
        public int WaitingTime
        {
            get
            {
                string strMinScore = "";
                QRCode.GetRunParam(nameof(WaitingTime), ref strMinScore);
                return Convert.ToInt16(strMinScore);
            }
            set
            {
                if (value < 0 || value > 1000000) throw new ArgumentException("DM码开关的指令枚举必须为0-1");
                QRCode.SetRunParam(nameof(WaitingTime), value.ToString());
            }
        }
        /// <summary>
        /// 运行二维码识别算法
        /// </summary>
        /// <param name="targetMats"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool Run(IReadOnlyDictionary<string, Mat> targetMats, out List<C2DCodeInfo?> matchInfo, out Mat? successMat,
            out double? timeCost, out string? errorMsg)
        {
            //图像处理类
            CMvdImage? imageTarget = null;

            matchInfo = null;
            successMat = null;
            timeCost = 0;
            errorMsg = string.Empty;

            try
            {
                imageTarget = new CMvdImage();
                #region 识别过程

                Bitmap? bitmap = null;
                foreach (var targetMatPair in targetMats)
                {
                    bitmap = Tool.ConvertMatToBitmap(targetMatPair.Value, PixelFormat.Format8bppIndexed);
                    //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    //bitmap.Save("D:\\121.bmp");

                    imageTarget.InitImage(bitmap);

                    //CMvdRectangleF rectangleF = new CMvdRectangleF(bitmap.Width/2, bitmap.Height/2, bitmap.Width, bitmap.Height);

                    //限制条件：算法支持的最小图像宽度为32，最小图像高度为32，仅支持MVD_PIXEL_MONO_08的像素格式。

                    if (!imageTarget.IsEmpty && imageTarget.Width >= 32 && imageTarget.Height >= 32)
                    {
                        //图像格式不是8位灰度图像，则转换为8位灰度图像
                        if (imageTarget.PixelFormat != SupportImageFormat) imageTarget.ConvertImagePixelFormat(SupportImageFormat);

                        QRCode.InputImage = imageTarget;
                        //QRCode.ROI= rectangleF;
                        QRCode.Run();

                        timeCost = QRCode.GetAlgRunTime();
                        matchInfo = QRCode.Result.CodeInfoList;

                        //算法执行成功，且匹配成功
                        if (matchInfo != null&& matchInfo.Count>0)
                        {
                            successMat = targetMatPair.Value.Clone();
                            break;
                        }
                    }

                    bitmap.Dispose();
                }

                bitmap?.Dispose();

                #endregion
            }
            catch (MvdException ex)
            {
                errorMsg = $"MVD异常：{ex.ErrorCode:X}";
            }
            catch (Exception ex)
            {
                errorMsg = $"二维码执行出错：{ex.Message}";
            }
            finally
            {
                imageTarget?.Dispose();
            }

            if (matchInfo != null && successMat != null)
            {
                return true;
            }
            return false;

        }


        public void SetRun(QRCodeParam param)
        {
            this.Loc2DCodeNum = param.Loc2DCodeNum;
            this.MinBarSize = param.MinBarSize;
            this.MaxBarSize = param.MaxBarSize;

            this.MirrorMode = param.MirrorMode;
            this.SampleLevel = param.SampleLevel;
            this.Polarity = param.Polarity;
            this.DiscreteFlag = param.DiscreteFlag;
            this.DistortionFlag = param.DistortionFlag;
            this.RectangleFlag = param.RectangleFlag;
            this.AppMode = param.AppMode;
            this.CodeQRFlag = param.CodeQRFlag;
            this.CodeDMFlag = param.CodeDMFlag;
            this.WaitingTime = param.WaitingTime;
        }
        public void Dispose()
        {
            QRCode?.Dispose();
        }
        /// <summary>
        /// 轮廓模板参数
        /// <para>类库内部调用</para>
        /// </summary>
        public struct QRCodeParam
        {
            public int Loc2DCodeNum { get; }
            public int MinBarSize { get; }
            public int MaxBarSize { get; }
            public int MirrorMode { get; }
            public int SampleLevel { get; }
            public int Polarity { get; }
            public int DiscreteFlag { get; }
            public int DistortionFlag { get; }
            public int RectangleFlag { get; }
            public int AppMode { get; }
            public int CodeQRFlag { get; }
            public int CodeDMFlag { get; }
            public int WaitingTime { get; }

            public QRCodeParam(bool use,
                int Loc2DCodeNum = 5,
                int MinBarSize =40,
                int MaxBarSize = 300,
                int MirrorMode = 0,
                int SampleLevel = 1,
                int Polarity = 0,
                int DiscreteFlag = 2,
                int DistortionFlag =0,
                int RectangleFlag = 0,
                int AppMode = 1,
                int CodeQRFlag=1,
            int CodeDMFlag = 0,
                int WaitingTime = 1000)
            {
                this.Loc2DCodeNum = Loc2DCodeNum;
                this.MinBarSize = MinBarSize;
                this.MaxBarSize = MaxBarSize;
                this.MirrorMode = MirrorMode;
                this.SampleLevel = SampleLevel;
                this.Polarity = Polarity;
                this.DiscreteFlag = DiscreteFlag;
                this.DistortionFlag = DistortionFlag;
                this.RectangleFlag = RectangleFlag;
                this.AppMode = AppMode;
                this.CodeQRFlag = CodeQRFlag;
                this.CodeDMFlag = CodeDMFlag;
                this.WaitingTime = WaitingTime;
            }
        }
        #endregion
    }
}
