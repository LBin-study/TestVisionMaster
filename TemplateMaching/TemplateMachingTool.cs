using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;
using VisionDesigner.ContourPatMatch;

namespace TestVisionMaster.TemplateMaching
{
    internal class TemplateMachingTool
    {
        /// <summary>
        /// 支持的图像格式(仅支持8位灰度图像)
        /// </summary>
        private const MVD_PIXEL_FORMAT SupportImageFormat = MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08;

        private readonly CContourPatMatchTool ContourMatch;
        /// <summary>
        /// 模板字典。初始创建时，使用时间条码作为key，加载时，使用模板路径作为Key
        /// </summary>
        private Dictionary<string, CContourPattern> PatternS = new Dictionary<string, CContourPattern>();

        private readonly object _runLock = new object();
        public bool CanUse { get; }=false;

        public uint TemplateCount => ContourMatch.GetPatternNum();

        #region 运行参数

        /// <summary>
        /// 金字塔模板匹配最小匹配分数
        /// <para>范围0~1。默认值为0.5</para>
        /// </summary>
        public float MinScore
        {
            get
            {
                string strMinScore = "";
                ContourMatch.GetRunParam(nameof(MinScore), ref strMinScore);
                return Convert.ToSingle(strMinScore);
            }
            set
            {
                if (value < 0 || value > 1) throw new ArgumentException("分数范围必须在0-1之间");
                ContourMatch.SetRunParam(nameof(MinScore), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配最大匹配个数
        /// <para>范围1~50000。默认值为1</para>
        /// </summary>
        public ushort MaxMatchNum
        {
            get
            {
                string? strMaxMatchNum = null;
                ContourMatch.GetRunParam(nameof(MaxMatchNum), ref strMaxMatchNum);
                return Convert.ToUInt16(strMaxMatchNum);
            }
            set
            {
                if (value < 1 || value > 50000) throw new ArgumentException("最大匹配个数范围必须在1-50000之间");
                ContourMatch.SetRunParam(nameof(MaxMatchNum), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配极性（是否考虑极性)
        /// <para>0:No,不考虑极性；1:Yes,考虑极性。默认值为1</para>
        /// </summary>
        public bool Polarity
        {
            get
            {
                string? strPolarity = null;
                ContourMatch.GetRunParam(nameof(Polarity), ref strPolarity);
                return strPolarity == "Yes";
            }
            set => ContourMatch.SetRunParam(nameof(Polarity), value ? "Yes" : "No");
        }

        /// <summary>
        /// 金字塔模板匹配起始角度
        /// <para>范围-180~180。默认值为-180</para>
        /// </summary>
        public short AngleStart
        {
            get
            {
                string? strAngleStart = null;
                ContourMatch.GetRunParam(nameof(AngleStart), ref strAngleStart);
                return Convert.ToInt16(strAngleStart);
            }
            set
            {
                if (value < -180 || value > 180) throw new ArgumentException("起始角度范围必须在-180-180之间");
                ContourMatch.SetRunParam(nameof(AngleStart), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配终止角度
        /// <para>范围-180~180。默认值为-180</para>
        /// </summary>
        public short AngleEnd
        {
            get
            {
                string? strAngleEnd = null;
                ContourMatch.GetRunParam(nameof(AngleEnd), ref strAngleEnd);
                return Convert.ToInt16(strAngleEnd);
            }
            set
            {
                if (value < -180 || value > 180) throw new ArgumentException("起始角度范围必须在-180-180之间");
                ContourMatch.SetRunParam(nameof(AngleEnd), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配起始尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleStart
        {
            get
            {
                string? strScaleStart = null;
                ContourMatch.GetRunParam(nameof(ScaleStart), ref strScaleStart);
                return Convert.ToSingle(strScaleStart);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("起始尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleStart), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配终止尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleEnd
        {
            get
            {
                string? strScaleEnd = null;
                ContourMatch.GetRunParam(nameof(ScaleEnd), ref strScaleEnd);
                return Convert.ToSingle(strScaleEnd);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("终止尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleEnd), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配噪点标记（是否考虑噪点）
        /// <para>True为1；False为0。默认值False</para>
        /// </summary>
        public bool SpotterFlag
        {
            get
            {
                string? strSpotterFlag = null;
                ContourMatch.GetRunParam(nameof(SpotterFlag), ref strSpotterFlag);
                return strSpotterFlag == "1";
            }
            set => ContourMatch.SetRunParam(nameof(SpotterFlag), Tool.ToIntStr(value));
        }

        /// <summary>
        /// 金字塔模板匹配最大重叠率
        /// <para>范围0~100。默认40</para>
        /// </summary>
        public byte MaxOverlap
        {
            get
            {
                string? strMaxOverlap = null;
                ContourMatch.GetRunParam(nameof(MaxOverlap), ref strMaxOverlap);
                return Convert.ToByte(strMaxOverlap);
            }
            set
            {
                if (value > 100) throw new ArgumentException("最大重叠率范围必须在0-100之间");
                ContourMatch.SetRunParam(nameof(MaxOverlap), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配X起始尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleXStart
        {
            get
            {
                string? strScaleXStart = null;
                ContourMatch.GetRunParam(nameof(ScaleXStart), ref strScaleXStart);
                return Convert.ToSingle(strScaleXStart);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("X起始尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleXStart), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配X终止尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleXEnd
        {
            get
            {
                string? strScaleXEnd = null;
                ContourMatch.GetRunParam(nameof(ScaleXEnd), ref strScaleXEnd);
                return Convert.ToSingle(strScaleXEnd);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("X终止尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleXEnd), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配Y起始尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleYStart
        {
            get
            {
                string? strScaleYStart = null;
                ContourMatch.GetRunParam(nameof(ScaleYStart), ref strScaleYStart);
                return Convert.ToSingle(strScaleYStart);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("Y起始尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleYStart), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配Y终止尺度
        /// <para>范围0.1~10。默认值为1</para>
        /// </summary>
        public float ScaleYEnd
        {
            get
            {
                string? strScaleYEnd = null;
                ContourMatch.GetRunParam(nameof(ScaleYEnd), ref strScaleYEnd);
                return Convert.ToSingle(strScaleYEnd);
            }
            set
            {
                if (value < 0.1 || value > 10) throw new ArgumentException("Y终止尺度范围必须在0.1-10之间");
                ContourMatch.SetRunParam(nameof(ScaleYEnd), value.ToString("F2"));
            }
        }

        /// <summary>
        /// 金字塔模板匹配延拓阈值
        /// <para>范围0~90。默认值为0</para>
        /// </summary>
        public byte MatchExtentRate
        {
            get
            {
                string? strMatchExtentRate = null;
                ContourMatch.GetRunParam(nameof(MatchExtentRate), ref strMatchExtentRate);
                return Convert.ToByte(strMatchExtentRate);
            }
            set
            {
                if (value > 90) throw new ArgumentException("延拓阈值范围必须在0-90之间");
                ContourMatch.SetRunParam(nameof(MatchExtentRate), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配匹配粗搜最小得分阈值开关
        /// <para>True为1；False为0。默认值False</para>
        /// </summary>
        public bool RoughMinScoreFlag
        {
            get
            {
                string? strRoughMinScoreFlag = null;
                ContourMatch.GetRunParam(nameof(RoughMinScoreFlag), ref strRoughMinScoreFlag);
                return strRoughMinScoreFlag == "1";
            }
            set => ContourMatch.SetRunParam(nameof(RoughMinScoreFlag), Tool.ToIntStr(value));
        }

        /// <summary>
        /// 金字塔模板匹配匹配粗搜最小得分阈值
        /// <para>范围0~100。默认值为50</para>
        /// </summary>
        public byte RoughMinScore
        {
            get
            {
                string? strRoughMinScore = null;
                ContourMatch.GetRunParam(nameof(RoughMinScore), ref strRoughMinScore);
                return Convert.ToByte(strRoughMinScore);
            }
            set
            {
                if (value > 100) throw new ArgumentException("金字塔模板匹配匹配粗搜最小得分阈值范围必须在0-100之间");
                ContourMatch.SetRunParam(nameof(RoughMinScore), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配排序类型，分为7种：
        /// <para>1 - 不进行排序：None</para>
        /// <para>2 - 按分数降序排序：Score</para>
        /// <para>3 - 按角度降序排序：Angle</para>
        /// <para>4 - 按X由小到大排序：X</para>
        /// <para>5 - 按Y由小到大排序：Y</para>
        /// <para>6 - 先按X由小到大排序，再按Y由小到大排序：XY</para>
        /// <para>7 - 先按Y由小到大排序，再按X由小到大排序：YX</para>
        /// <para>默认值是2</para>
        /// </summary>
        public byte SortType
        {
            get
            {
                string? strSortType = null;
                ContourMatch.GetRunParam(nameof(SortType), ref strSortType);
                switch (strSortType)
                {
                    case "None":
                        return 1;

                    case "Score":
                        return 2;

                    case "Angle":
                        return 3;

                    case "X":
                        return 4;

                    case "Y":
                        return 5;

                    case "XY":
                        return 6;

                    case "YX":
                        return 7;

                    default:
                        return 2;
                }
            }
            set
            {
                if (value < 1 || value > 7) throw new ArgumentException("排序类型范围必须在1-7之间");
                switch (value)
                {
                    case 1:
                        ContourMatch.SetRunParam(nameof(SortType), "None");
                        break;

                    case 2:
                        ContourMatch.SetRunParam(nameof(SortType), "Score");
                        break;

                    case 3:
                        ContourMatch.SetRunParam(nameof(SortType), "Angle");
                        break;

                    case 4:
                        ContourMatch.SetRunParam(nameof(SortType), "X");
                        break;

                    case 5:
                        ContourMatch.SetRunParam(nameof(SortType), "Y");
                        break;

                    case 6:
                        ContourMatch.SetRunParam(nameof(SortType), "XY");
                        break;

                    case 7:
                        ContourMatch.SetRunParam(nameof(SortType), "YX");
                        break;
                }
            }
        }

        /// <summary>
        /// 金字塔模板匹配超时控制
        /// <para>范围0~10000.默认值为0</para>
        /// </summary>
        public ushort TimeOut
        {
            get
            {
                string? strTimeOut = null;
                ContourMatch.GetRunParam(nameof(TimeOut), ref strTimeOut);
                return Convert.ToUInt16(strTimeOut);
            }
            set
            {
                if (value > 10000) throw new ArgumentException("超时控制范围必须在0-10000之间");
                ContourMatch.SetRunParam(nameof(TimeOut), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配边缘阈值标志，分为3种：
        /// <para>0 - 自适应阈值：Auto </para>
        /// <para>1 - 模板阈值：Model </para>
        /// <para>2 - 手动阈值：Manual </para>
        /// <para>默认值是0</para>
        /// </summary>
        public byte MatchThresholdFlag
        {
            get
            {
                string? strMatchThresholdFlag = null;
                ContourMatch.GetRunParam(nameof(MatchThresholdFlag), ref strMatchThresholdFlag);
                switch (strMatchThresholdFlag)
                {
                    case "Auto":
                        return 0;

                    case "Model":
                        return 1;

                    case "Manual":
                        return 2;

                    default:
                        return 0;
                }
            }
            set
            {
                if (value > 2) throw new ArgumentException("边缘阈值标志范围必须在0-2之间");
                switch (value)
                {
                    case 0:
                        ContourMatch.SetRunParam(nameof(MatchThresholdFlag), "Auto");
                        break;

                    case 1:
                        ContourMatch.SetRunParam(nameof(MatchThresholdFlag), "Model");
                        break;

                    case 2:
                        ContourMatch.SetRunParam(nameof(MatchThresholdFlag), "Manual");
                        break;
                }
            }
        }

        /// <summary>
        /// 金字塔模板匹配边缘阈值
        /// <para>范围1~255。默认值为40</para>
        /// </summary>
        public byte MatchThresholdHigh
        {
            get
            {
                string? strMatchThresholdHigh = null;
                ContourMatch.GetRunParam(nameof(MatchThresholdHigh), ref strMatchThresholdHigh);
                return Convert.ToByte(strMatchThresholdHigh);
            }
            set
            {
                if (value < 1) throw new ArgumentException("边缘阈值范围必须在1-255之间");
                ContourMatch.SetRunParam(nameof(MatchThresholdHigh), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配起始横切角度
        /// <para>范围-45~45。默认值为0</para>
        /// </summary>
        public short SkewXStart
        {
            get
            {
                string? strSkewXStart = null;
                ContourMatch.GetRunParam(nameof(SkewXStart), ref strSkewXStart);
                return Convert.ToInt16(strSkewXStart);
            }
            set
            {
                if (value < -45 || value > 45) throw new ArgumentException("起始横切角度范围必须在-45-45之间");
                ContourMatch.SetRunParam(nameof(SkewXStart), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配终止横切角度
        /// <para>范围-45~45。默认值为0</para>
        /// </summary>
        public short SkewXEnd
        {
            get
            {
                string? strSkewXEnd = null;
                ContourMatch.GetRunParam(nameof(SkewXEnd), ref strSkewXEnd);
                return Convert.ToInt16(strSkewXEnd);
            }
            set
            {
                if (value < -45 || value > 45) throw new ArgumentException("终止横切角度范围必须在-45-45之间");
                ContourMatch.SetRunParam(nameof(SkewXEnd), value.ToString());
            }
        }

        /// <summary>
        /// 金字塔模板匹配多模板最大重叠率
        /// <para>范围0~100。默认值为40</para>
        /// </summary>
        public byte MultiModelMaxOverlap
        {
            get
            {
                string? strMultiModelMaxOverlap = null;
                ContourMatch.GetRunParam(nameof(MultiModelMaxOverlap), ref strMultiModelMaxOverlap);
                return Convert.ToByte(strMultiModelMaxOverlap);
            }
            set
            {
                if (value > 100) throw new ArgumentException("多模板最大重叠率范围必须在0-100之间");
                ContourMatch.SetRunParam(nameof(MultiModelMaxOverlap), value.ToString());
            }
        }

        #endregion

        public TemplateMachingTool()
        {
            try
            {
                ContourMatch = new CContourPatMatchTool();
                CanUse=true;
            }
            catch (MvdException )
            {
                CanUse =false;
            }
            catch (Exception )
            {
                CanUse = false;
            }
        }
        /// <summary>
        /// 运行模板匹配算法
        /// </summary>
        /// <param name="targetMats"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool Run(IReadOnlyDictionary<string, Mat> targetMats, out List<CContourMatchInfo?> matchInfo,out Mat? successMat,
            out double? timeCost,out string? errorMsg)
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

                #region 匹配过程

                Bitmap? bitmap = null;
                lock (_runLock)
                {
                    if (ContourMatch.GetPatternNum() == 0) throw new Exception("模板设置异常");

                    //锁确保多线程安全，且包括整个算法过程，减少上下文切换

                    foreach (var targetMatPair in targetMats)
                    {
                        bitmap = Tool.ConvertMatToBitmap(targetMatPair.Value, PixelFormat.Format8bppIndexed);

                        // 加载匹配图像
                        imageTarget.InitImage(bitmap);

                        //限制条件：算法支持的最小图像宽度为32，最小图像高度为32，仅支持MVD_PIXEL_MONO_08的像素格式。

                        if (!imageTarget.IsEmpty && imageTarget.Width >= 32 && imageTarget.Height >= 32)
                        {
                            //图像格式不是8位灰度图像，则转换为8位灰度图像
                            if (imageTarget.PixelFormat != SupportImageFormat) imageTarget.ConvertImagePixelFormat(SupportImageFormat);

                            ContourMatch.InputImage = imageTarget;
                            // 执行模板匹配处理
                            ContourMatch.Run();

                            timeCost = ContourMatch.GetAlgRunTime();
                            matchInfo = ContourMatch.Result.MatchInfoList;

                            //算法执行成功，且匹配成功
                            if (ContourMatch.RunStatus == MvdRunStatus.MvdRunSuccess && matchInfo != null)
                            {
                                successMat = targetMatPair.Value.Clone();
                                break;
                            }
                        }

                        bitmap.Dispose();
                    }
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
                errorMsg = $"匹配出错：{ex.Message}";
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



        #region 参数模板

        public bool SetTemplate(IReadOnlyList<ContourPatternParam> contourPatternParamList)
        {
            if (contourPatternParamList.Count == 0 || ContourMatch == null) return false;

            try
            {
                // 清空匹配模板
                ContourMatch.ClearPatterns();
                foreach (var contourPatternParam in contourPatternParamList)
                {
                    var contourPattern = CreateContourPattern(contourPatternParam, out CContourPattern Resultpattern);
                    if (!contourPattern) continue;
                    ContourMatch.AddPattern(Resultpattern);
                }
                return true;
            }
            catch (MvdException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CreateContourPattern(ContourPatternParam contourPatternParam,out CContourPattern Resultpattern)
        {
            //模板图像
            Bitmap? bitmapTemplate = null;
            //图像处理类
            CMvdImage? imageTemplate = null;
            Resultpattern = new CContourPattern();
            try
            {
                imageTemplate = new CMvdImage();
                bitmapTemplate = Tool.ConvertMatToBitmap(contourPatternParam.ImageData, PixelFormat.Format8bppIndexed);
                // 加载模板图像
                imageTemplate.InitImage(bitmapTemplate);

                if (imageTemplate.IsEmpty) throw new Exception("模板图像为空");

                //图像格式不是8位灰度图像，则转换为8位灰度图像
                if (imageTemplate.PixelFormat != SupportImageFormat) imageTemplate.ConvertImagePixelFormat(SupportImageFormat);

                //保存模板图像到本地
                //imageTemplate.SaveImage(Path.ChangeExtension(templateFile, ".tiff"), MVD_FILE_FORMAT.MVD_FILE_TIFF);

                var contourPattern = new CContourPattern();
                contourPattern.InputImage = imageTemplate;

                // 设置建模基准点
                contourPattern.BasicParam.FixPoint = new MVD_POINT_F(Convert.ToSingle(imageTemplate.Width) / 2, Convert.ToSingle(imageTemplate.Height) / 2);

                #region 设置参数

                contourPattern.SetRunParam(nameof(ContourPatternParam.PyramidScaleFlag),Tool.ToIntStr(contourPatternParam.PyramidScaleFlag));
                contourPattern.SetRunParam(nameof(ContourPatternParam.PyramidScaleLevel), contourPatternParam.PyramidScaleLevel.ToString());
                contourPattern.SetRunParam(nameof(ContourPatternParam.PyramidScaleRLevel), contourPatternParam.PyramidScaleRLevel.ToString());
                contourPattern.SetRunParam(nameof(ContourPatternParam.EdgeThresholdFlag), Tool.ToIntStr(contourPatternParam.EdgeThresholdFlag));
                contourPattern.SetRunParam(nameof(ContourPatternParam.WeightMaskFlag), Tool.ToIntStr(contourPatternParam.WeightMaskFlag));
                contourPattern.SetRunParam(nameof(ContourPatternParam.ChainFlag), Tool.ToIntStr(contourPatternParam.ChainFlag));
                contourPattern.SetRunParam(nameof(ContourPatternParam.MinChainLen), contourPatternParam.MinChainLen.ToString());
                contourPattern.SetRunParam(nameof(ContourPatternParam.MaskModelFlag), Tool.ToIntStr(contourPatternParam.MaskModelFlag));
                contourPattern.SetRunParam(nameof(ContourPatternParam.IncludeImageFlag), contourPatternParam.IncludeImageFlag ? "Include" : "Exclude");

                #endregion

                // 执行模型训练处理
                contourPattern.Train();

                // 保存模型到本地
                //contourPattern.ExportPattern(Path.ChangeExtension("D:\\1", ".contourmxml"));
                ContourMatch.AddPattern(contourPattern);
                return true;
            }
            catch (MvdException )
            {
                return false;
            }
            catch (Exception )
            {
                return false;
            }
            finally
            {
                //及时释放资源，防止内存泄漏
                imageTemplate?.Dispose();
                bitmapTemplate?.Dispose();
            }
        }

        

        #endregion

        public void Dispose()
        {
            ContourMatch?.Dispose();
        }
        /// <summary>
        /// 轮廓模板参数
        /// <para>类库内部调用</para>
        /// </summary>
        public struct ContourPatternParam
        {
            /// <summary>
            /// 金字塔模板模型特征尺度标记
            /// <para>False:手动；True:自动。默认True</para>
            /// </summary>
            public bool PyramidScaleFlag { get; }

            /// <summary>
            /// //金字塔模板模型层数
            /// <para>1~10,默认5</para>
            /// </summary>
            public byte PyramidScaleLevel { get; }

            /// <summary>
            /// 金字塔模板模型返回层数
            /// <para>1~10,默认1</para>
            /// </summary>
            public byte PyramidScaleRLevel { get; }

            /// <summary>
            /// 金字塔模板模型边缘阈值标记
            /// <para>False:手动；True:自动。默认True</para>
            /// </summary>
            public bool EdgeThresholdFlag { get; }

            /// <summary>
            /// 金字塔模板模型边缘阈值
            /// <para>1~255,默认15</para>
            /// </summary>
            public byte EdgeThreshold { get; }

            /// <summary>
            /// 权重掩膜使能标记
            /// <para>False:不使用；True:使用。默认False</para>
            /// </summary>
            public bool WeightMaskFlag { get; }

            /// <summary>
            /// 链长使能标记
            /// <para>False:手动；True:自动。默认True</para>
            /// </summary>
            public bool ChainFlag { get; }

            /// <summary>
            /// 最小链长长度
            /// <para>1~1000,默认4</para>
            /// </summary>
            public ushort MinChainLen { get; }

            /// <summary>
            /// 掩膜形状建模使能
            /// <para>False:不使用；True:使用。默认False</para>
            /// </summary>
            public bool MaskModelFlag { get; }

            /// <summary>
            /// 高精度模板是否包含图像标记
            /// <para>False:不包含；True:包含。默认True</para>
            /// </summary>
            public bool IncludeImageFlag { get; }

            /// <summary>
            /// 图像数据
            /// </summary>
            public Mat ImageData { get; set; }

            public ContourPatternParam(Mat imageData,
                bool pyramidScaleFlag = true,
                byte pyramidScaleLevel = 5,
                byte pyramidScaleRLevel = 1,
                bool edgeThresholdFlag = true,
                byte edgeThreshold = 15,
                bool weightMaskFlag = false,
                bool chainFlag = true,
                ushort minChainLen = 4,
                bool maskModelFlag = false,
                bool includeImageFlag = true)
            {
                if (pyramidScaleLevel < 1 || pyramidScaleLevel > 10) throw new ArgumentOutOfRangeException(nameof(pyramidScaleLevel));
                if (pyramidScaleRLevel < 1 || pyramidScaleRLevel > 10) throw new ArgumentOutOfRangeException(nameof(pyramidScaleRLevel));
                if (edgeThreshold < 1) throw new ArgumentOutOfRangeException(nameof(edgeThreshold));
                if (minChainLen < 1 || minChainLen > 1000) throw new ArgumentOutOfRangeException(nameof(minChainLen));

                if (imageData == null) throw new ArgumentException(nameof(imageData));

                ImageData = imageData;
                PyramidScaleFlag = pyramidScaleFlag;
                PyramidScaleLevel = pyramidScaleLevel;
                PyramidScaleRLevel = pyramidScaleRLevel;
                EdgeThresholdFlag = edgeThresholdFlag;
                EdgeThreshold = edgeThreshold;
                WeightMaskFlag = weightMaskFlag;
                ChainFlag = chainFlag;
                MinChainLen = minChainLen;
                MaskModelFlag = maskModelFlag;
                IncludeImageFlag = includeImageFlag;
            }
        }

    }
}
