using OpenCvSharp;
using OpenCvSharp.Detail;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using TestVisionMaster.TemplateMaching;
using VisionDesigner.Code2DReader;
using VisionDesigner.ContourPatMatch;
using static TestVisionMaster.QRCodeTool;
using static TestVisionMaster.TemplateMaching.TemplateMachingTool;

namespace TestVisionMaster
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            
            Program program = new Program();
            program.QRCodeToolTest();
        }
        public void QRCodeToolTest()
        {
            string TestImagePath = "E:\\123.jpg";
            Mat mat = Cv2.ImRead(TestImagePath);
            // 定义截取区域
            int x = 500;
            int y = 1000;
            int width = 400;
            int height = 400;
            Rect roi = new Rect(x, y, width, height);
            Mat croppedMat = mat[roi];
            Mat grayMat = new Mat();
            Cv2.CvtColor(croppedMat, grayMat, ColorConversionCodes.BGR2GRAY);

            QRCodeTool QRCode = new QRCodeTool();
            QRCodeParam param = new QRCodeParam(true);
            QRCode.SetRun(param);
            Dictionary<string, Mat> pairs = new Dictionary<string, Mat>();
            pairs.Add("Test", grayMat);
            grayMat.ImWrite("D:\\111.bmp");

            QRCode.Run(pairs, out List<C2DCodeInfo?> matchInfos, out Mat? successMat, out double? TimeCost, out string? ErrorMsg);
            C2DCodeInfo matchInfo = matchInfos.FirstOrDefault();
            //Cv2.ImWrite("D:\\test.bmp", Tool.CreatRotaRec(successMat, new Point2f(matchInfo.Center.nX, matchInfo.Center.nY), new Size2f(matchInfo., matchInfo.MatchBox.Height), matchInfo.MatchBox.Angle));
            Console.ReadKey();
        }
        public void TemplateMachingToolTest()
        {
            string TestImagePath = "E:\\5.bmp";
            Mat mat = Cv2.ImRead(TestImagePath);
            // 定义截取区域
            int x = 1000;
            int y = 1000;
            int width = 1000;
            int height = 800;
            Rect roi = new Rect(x, y, width, height);
            Mat croppedMat = mat[roi];
            Mat grayMat = new Mat();
            Cv2.CvtColor(croppedMat, grayMat, ColorConversionCodes.BGR2GRAY);

            // 将灰度图转换为字节数组
            byte[] encodedBytes;
            Cv2.ImEncode(".jpg", grayMat, out encodedBytes);

            TemplateMachingTool template = new TemplateMachingTool();
            ContourPatternParam param = new ContourPatternParam(encodedBytes);

            CContourPattern ResultPattern = new CContourPattern();
            template.CreateContourPattern(param, out ResultPattern);
            Dictionary<string, Mat> pairs = new Dictionary<string, Mat>();
            pairs.Add("Test", mat);
            template.Run(pairs, out List<CContourMatchInfo?> matchInfos, out Mat? successMat, out double? TimeCost, out string? ErrorMsg);
            CContourMatchInfo matchInfo = matchInfos.FirstOrDefault();
            Cv2.ImWrite("D:\\test.bmp", Tool.CreatRotaRec(successMat, new Point2f(matchInfo.MatchPoint.fX, matchInfo.MatchPoint.fY), new Size2f(matchInfo.MatchBox.Width, matchInfo.MatchBox.Height), matchInfo.MatchBox.Angle));
            Console.ReadKey();
        }

    }
}
