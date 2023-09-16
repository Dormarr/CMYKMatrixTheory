using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Aruco;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using Emgu.CV.XImgproc;
using System.Threading;
using Emgu.CV.Util;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Controls;
using System.Runtime.Intrinsics;
using System.Windows.Shapes;
using Emgu.CV.LineDescriptor;
using System.Linq;
using System.Windows.Input;
using System.Text;
using Emgu.CV.ML;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace QuayCodeV2
{
    public partial class Detect
    {
        public void IdentifyFromVideo()
        {
            VideoCapture vid = new VideoCapture(0, VideoCapture.API.DShow);

            Mat frame = new();
            bool pause = false;

            while (!pause)
            {
                vid.Read(frame);

                Mat[] outputArray = FindSquares(frame);




                CvInvoke.Imshow("Human Vision", outputArray[1]);
                //CvInvoke.Imshow("Camera Vision", outputArray[0]);

                int keypressed = CvInvoke.WaitKey(1);
                if (keypressed == 27)
                {
                    pause = true;
                }

            }
        }

        public Mat[] FindSquares(Mat input)
        {
            Mat frameGray = new();
            Mat threshMat = new();
            MCvScalar sclr = new MCvScalar(85,255,55);
            CvInvoke.CvtColor(input, frameGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.AdaptiveThreshold(frameGray, threshMat, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 11, 5);

            VectorOfVectorOfPoint cnts = new VectorOfVectorOfPoint();
            //CvInvoke.FindContours(threshMat, cnts, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            CvInvoke.FindContours(threshMat, cnts, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            List<PointF> cand = new List<PointF>();
            VectorOfPointF cnt = new VectorOfPointF();
            VectorOfPointF cnt2 = new VectorOfPointF();
            //CvInvoke.CvtColor(threshMat, threshMat, ColorConversion.Gray2Bgr); //CONFLICTS WITH CORNERSUBPIX

            for (int i = 0; i < cnts.Size; i++)
            {
                CvInvoke.ApproxPolyDP(cnts[i], cnt, 0.05 * CvInvoke.ArcLength(cnts[i], true), true);
                
                if (cnt.Size != 4 || CvInvoke.ContourArea(cnt) < 200 || !CvInvoke.IsContourConvex(cnt))
                {
                    continue;
                }

                //Below line throws error when quay gets too close, or 2 quays are introduced. It's a channel issue. Needs binary colour.
                CvInvoke.CornerSubPix(threshMat, cnt, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.01));


                cnt2 = OrderContour(cnt);

                CvInvoke.Circle(threshMat, PointFToPoint(cnt2[0]), 4, new MCvScalar(0, 0, 255), 7);
                //CvInvoke.Circle(threshMat, PointFToPoint(cnt[1]), 2, new MCvScalar(0, 255, 0), 2);
                //CvInvoke.Circle(threshMat, PointFToPoint(cnt[2]), 2, new MCvScalar(255, 0, 0), 2);
                //CvInvoke.Circle(threshMat, PointFToPoint(cnt[3]), 2, new MCvScalar(0, 255, 255), 2);

                //LABEL CORNERS
                //CvInvoke.PutText(threshMat, "0", PointFToPoint(cnt2[0]), FontFace.HersheySimplex, 1, sclr);
                //CvInvoke.PutText(threshMat, "1", PointFToPoint(cnt2[1]), FontFace.HersheySimplex, 1, sclr);
                //CvInvoke.PutText(threshMat, "2", PointFToPoint(cnt2[2]), FontFace.HersheySimplex, 1, sclr);
                //CvInvoke.PutText(threshMat, "3", PointFToPoint(cnt2[3]), FontFace.HersheySimplex, 1, sclr);

                //DRAW MID POINT
                int cx = (int)(cnt2[0].X + cnt2[1].X + cnt2[2].X + cnt2[3].X) / 4;
                int cy = (int)(cnt2[0].Y + cnt2[1].Y + cnt2[2].Y + cnt2[3].Y) / 4;
                CvInvoke.Circle(threshMat, new Point(cx, cy), 4, new MCvScalar(255,50,200));

                PointF[] pointsF = ConvertVectorOfPointToPointFArray(cnt2);

                for (int j = 0; j < pointsF.Length; j++)
                {
                    cand.Add(pointsF[j]);
                }
            }

            //CvInvoke.DrawContours(threshMat, cnts, -1, sclr);
            DrawContourFloat(threshMat, cand, new MCvScalar(100,255,100));
            
            if(cnt2.Size == 4)
            {
                GetContourBits(input, cnt2, 1024); //Bits needs to vary based on size.
                DrawContourFloat(input, cand, new MCvScalar(0, 255, 0));
            }

            Mat[] mats = new Mat[2];
            mats[0] = threshMat;
            mats[1] = input;

            return mats;
        }

        PointF[] ConvertVectorOfPointToPointFArray(VectorOfPointF vectorOfPoint)
        {
            PointF[] pointFArray = new PointF[vectorOfPoint.Size];

            for (int i = 0; i < vectorOfPoint.Size; i++)
            {
                PointF point = vectorOfPoint[i];
                pointFArray[i] = new PointF(point.X, point.Y);
            }

            return pointFArray;
        }

        VectorOfPointF OrderContour(VectorOfPointF cntV)
        {
            PointF[] cnt = ConvertVectorOfPointToPointFArray(cntV);

            float cx = (cnt[0].X + cnt[1].X + cnt[2].X + cnt[3].X) / 4.0f;
            float cy = (cnt[0].Y + cnt[1].Y + cnt[2].Y + cnt[3].Y) / 4.0f;

            // IMPORTANT! We assume the contour points are counter-clockwise (as we use EXTERNAL contours in findContours)
            if (cnt[0].X <= cx && cnt[0].Y <= cy)
            {
                Swap(ref cnt[1], ref cnt[3]);
            }
            else
            {
                Swap(ref cnt[0], ref cnt[1]);
                Swap(ref cnt[2], ref cnt[3]);
            }
            VectorOfPointF send = new VectorOfPointF(cnt);
            return send;
        }

        void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

         void DrawContourFloat(Mat img, List<PointF> cnt, MCvScalar color)
        {
            for (int i = 0; i < cnt.Count; ++i)
            {
                PointF from = cnt[i];
                PointF to = cnt[(i + 1) % cnt.Count];
                Line ln = new Line();
                CvInvoke.Line(img, PointFToPoint(from), PointFToPoint(to), color, 2);
            }
        }

         Point PointFToPoint(PointF pointF)
        {
            return new Point((int)Math.Round(pointF.X), (int)Math.Round(pointF.Y));
        }

         void GetContourBits(Mat image, VectorOfPointF cnt, int bits)
        {
            //corners = PointF(0,0), PointF(bits, 0), PointF(bits, bits), PointF(0, bits);


            //CvInvoke.GetPerspectiveTransform
            //CvInvoke.WarpPerspective

            int pixelLen = (int)Math.Sqrt(bits);

            PointF[] corners = new PointF[4] {new PointF(0,0), new PointF(bits, 0), new PointF(bits, bits), new PointF(0, bits) };
            VectorOfPointF cornerV = new VectorOfPointF(corners);

            Mat m = CvInvoke.GetPerspectiveTransform(cnt, cornerV);
            Mat binary = new();
            CvInvoke.WarpPerspective(image, binary, m, new Size(bits, bits));
            CvInvoke.Threshold(binary, binary, 64, 255, ThresholdType.Binary);// | ThresholdType.Otsu);



            //Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(0, 0));
            //CvInvoke.Erode(binary, binary, element, new Point(0, 0), 4, BorderType.Constant, new MCvScalar(255, 255, 255));

            //Need to determine size before drawing grid.
            //Could run through different grid sizes, determining where it goes black white black white along top?
            //Once one of the three templates satisfies that, then you proceed to decode the rest.
            int scaleFactor = DetermineSize(binary);

            if(scaleFactor != 100)
            {
                CvInvoke.PutText(image, scaleFactor.ToString(), PointFToPoint(cnt[0]), FontFace.HersheyPlain, 2, new MCvScalar(0, 0, 0));
                DrawFullGrid(binary, scaleFactor, scaleFactor, new MCvScalar(0, 0, 255, 100));
                //binary.Save("C:\\Users\\Ryan\\Desktop\\Software Testing Ground\\Spam\\bin" + DateTime.Now.Ticks + ".png");

                //NOW WE HAVE SIZE, WE CAN JUST DECODE.
                //add header reading ability.
                string read = ReadCode(binary, scaleFactor);
                CvInvoke.PutText(image, " " + read, PointFToPoint(cnt[2]), FontFace.HersheyPlain, 1.2, new MCvScalar(0, 0, 0));

            }

            int keypressed = CvInvoke.WaitKey(1);
            if (keypressed == 88)
            {
                binary.Save("C:\\Users\\Ryan\\Desktop\\Software Testing Ground\\Spam\\bin" + DateTime.Now.Ticks + ".png");
            }
        }

        static int DetermineSize(Mat image)
        {
            if (CheckForSize(image, 64, 16))
            {
                return 16;
            }
            else if (CheckForSize(image, 46, 22))
            {
                return 22;
            }
            else if (CheckForSize(image, 28, 36))
            {
                return 36;
            }
            else
            {
                return 100;
            }
        }

        static bool CheckForSize(Mat image, int metric, int size)
        {
            int pixelLen = metric;
            List<char> chars = new List<char>();

            for(int r = 2; r <= 2; ++r)
            {
                for(int c = 0; c < size; ++c)
                {
                    int y = r * pixelLen + (pixelLen / 2);
                    int x = c * pixelLen + (pixelLen / 2);

                    if(image.GetRawData(y, x)[0] >= 128)
                    {
                        chars.Add('W');
                    }
                    else
                    {
                        chars.Add('B');
                    }
                }
            }
            StringBuilder charsToString = new StringBuilder();
            foreach(char c in chars)
            {
                charsToString.Append(c);
            }

            string strArray = "";
            string str12 = "BWBWBWBWBWBWBBWB";
            string str18 = "BWBWBWBWBWBWBWBWBWBBWB";
            string str32 = "BWBWBWBWBWBWBWBWBWBWBWBWBWBWBWBBBBWB";

            switch (size)
            {
                case 16:
                    strArray = str12;
                    break;
                case 22:
                    strArray = str18;
                    break;
                case 36:
                    strArray = str32;
                    break;
                    
            }            
            if(strArray == charsToString.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void DrawFullGrid(Mat img, int rows, int cols, MCvScalar color)
        {
            int cellW = img.Cols / cols;
            int cellH = img.Rows / rows;

            for(int i = 1; i < rows; i++)
            {
                int y = i * cellH;
                CvInvoke.Line(img, new Point(0, y), new Point(img.Cols , y), color, 4);
            }
            for(int i = 1; i < cols; i++)
            {
                int x = i * cellW;
                CvInvoke.Line(img, new Point(x, 0), new Point(x, img.Rows), color, 4);
            }
        }

        string ReadCode(Mat image, int sizeMetric)
        {

            int pixelLen = (int)1024 / sizeMetric;
            List<string> rawRead = new List<string>();

            byte[] blue = new byte[] { 255, 0, 0 };
            byte[] brightBlue = new byte[] { 255, 255, 0 };
            byte[] red = new byte[] { 0, 0, 255 };
            byte[] brightRed = new byte[] { 255, 0, 255 };
            byte[] yellow = new byte[] { 0, 255, 255 };
            byte[] black = new byte[] { 0, 0, 0 };
            byte[] white = new byte[] { 255, 255, 255 };

            (int, int)[] dataArray = new (int, int)[] { };
            (int, int)[] dataArray12 = new (int, int)[] { (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13) };
            (int, int)[] dataArray18 = new (int, int)[] { (8, 4), (9, 4), (10, 4), (11, 4), (12, 4), (13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (19, 4), (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (17, 5), (18, 5), (19, 5), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (14, 6), (15, 6), (16, 6), (17, 6), (18, 6), (19, 6), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (19, 7), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (15, 8), (16, 8), (17, 8), (18, 8), (19, 8), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (14, 9), (15, 9), (16, 9), (17, 9), (18, 9), (19, 9), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (15, 10), (16, 10), (17, 10), (18, 10), (19, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (14, 11), (15, 11), (16, 11), (17, 11), (18, 11), (19, 11), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (14, 12), (15, 12), (16, 12), (17, 12), (18, 12), (19, 12), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (13, 13), (14, 13), (15, 13), (16, 13), (17, 13), (18, 13), (19, 13), (3, 14), (4, 14), (5, 14), (6, 14), (7, 14), (8, 14), (9, 14), (10, 14), (11, 14), (12, 14), (13, 14), (3, 15), (4, 15), (5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15), (11, 15), (12, 15), (13, 15), (3, 16), (4, 16), (5, 16), (6, 16), (7, 16), (8, 16), (9, 16), (10, 16), (11, 16), (12, 16), (13, 16), (3, 17), (4, 17), (5, 17), (6, 17), (7, 17), (8, 17), (9, 17), (10, 17), (11, 17), (12, 17), (13, 17), (3, 18), (4, 18), (5, 18), (6, 18), (7, 18), (8, 18), (9, 18), (10, 18), (11, 18), (12, 18), (13, 18), (4, 19), (5, 19), (6, 19), (7, 19), (8, 19), (9, 19), (10, 19) };
            (int, int)[] dataArray32 = new (int, int)[] { (8, 4), (9, 4), (10, 4), (11, 4), (12, 4), (13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (19, 4), (20, 4), (21, 4), (22, 4), (23, 4), (24, 4), (25, 4), (26, 4), (27, 4), (28, 4), (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (17, 5), (18, 5), (19, 5), (20, 5), (21, 5), (22, 5), (23, 5), (24, 5), (25, 5), (26, 5), (27, 5), (28, 5), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (14, 6), (15, 6), (16, 6), (17, 6), (18, 6), (19, 6), (20, 6), (21, 6), (22, 6), (23, 6), (24, 6), (25, 6), (26, 6), (27, 6), (28, 6), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (19, 7), (20, 7), (21, 7), (22, 7), (23, 7), (24, 7), (25, 7), (26, 7), (27, 7), (28, 7), (29, 7), (30, 7), (31, 7), (32, 7), (33, 7), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (15, 8), (16, 8), (17, 8), (18, 8), (19, 8), (20, 8), (21, 8), (22, 8), (23, 8), (24, 8), (25, 8), (26, 8), (27, 8), (28, 8), (29, 8), (30, 8), (31, 8), (32, 8), (33, 8), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (14, 9), (15, 9), (16, 9), (17, 9), (18, 9), (19, 9), (20, 9), (21, 9), (22, 9), (23, 9), (24, 9), (25, 9), (26, 9), (27, 9), (28, 9), (29, 9), (30, 9), (31, 9), (32, 9), (33, 9), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (15, 10), (16, 10), (17, 10), (18, 10), (19, 10), (20, 10), (21, 10), (22, 10), (23, 10), (24, 10), (25, 10), (26, 10), (27, 10), (28, 10), (29, 10), (30, 10), (31, 10), (32, 10), (33, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (14, 11), (15, 11), (16, 11), (17, 11), (18, 11), (19, 11), (20, 11), (21, 11), (22, 11), (23, 11), (24, 11), (25, 11), (26, 11), (27, 11), (28, 11), (29, 11), (30, 11), (31, 11), (32, 11), (33, 11), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (14, 12), (15, 12), (16, 12), (17, 12), (18, 12), (19, 12), (20, 12), (21, 12), (22, 12), (23, 12), (24, 12), (25, 12), (26, 12), (27, 12), (28, 12), (29, 12), (30, 12), (31, 12), (32, 12), (33, 12), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (13, 13), (14, 13), (15, 13), (16, 13), (17, 13), (18, 13), (19, 13), (20, 13), (21, 13), (22, 13), (23, 13), (24, 13), (25, 13), (26, 13), (27, 13), (28, 13), (29, 13), (30, 13), (31, 13), (32, 13), (33, 13), (3, 14), (4, 14), (5, 14), (6, 14), (7, 14), (8, 14), (9, 14), (10, 14), (11, 14), (12, 14), (13, 14), (14, 14), (15, 14), (16, 14), (17, 14), (18, 14), (19, 14), (20, 14), (21, 14), (22, 14), (23, 14), (24, 14), (25, 14), (26, 14), (27, 14), (28, 14), (29, 14), (30, 14), (31, 14), (32, 14), (33, 14), (3, 15), (4, 15), (5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15), (11, 15), (12, 15), (13, 15), (14, 15), (22, 15), (23, 15), (24, 15), (25, 15), (26, 15), (27, 15), (28, 15), (29, 15), (30, 15), (31, 15), (32, 15), (33, 15), (3, 16), (4, 16), (5, 16), (6, 16), (7, 16), (8, 16), (9, 16), (10, 16), (11, 16), (12, 16), (13, 16), (14, 16), (22, 16), (23, 16), (24, 16), (25, 16), (26, 16), (27, 16), (28, 16), (29, 16), (30, 16), (31, 16), (32, 16), (33, 16), (3, 17), (4, 17), (5, 17), (6, 17), (7, 17), (8, 17), (9, 17), (10, 17), (11, 17), (12, 17), (13, 17), (14, 17), (22, 17), (23, 17), (24, 17), (25, 17), (26, 17), (27, 17), (28, 17), (29, 17), (30, 17), (31, 17), (32, 17), (33, 17), (3, 18), (4, 18), (5, 18), (6, 18), (7, 18), (8, 18), (9, 18), (10, 18), (11, 18), (12, 18), (13, 18), (14, 18), (22, 18), (23, 18), (24, 18), (25, 18), (26, 18), (27, 18), (28, 18), (29, 18), (30, 18), (31, 18), (32, 18), (33, 18), (3, 19), (4, 19), (5, 19), (6, 19), (7, 19), (8, 19), (9, 19), (10, 19), (11, 19), (12, 19), (13, 19), (14, 19), (22, 19), (23, 19), (24, 19), (25, 19), (26, 19), (27, 19), (28, 19), (29, 19), (30, 19), (31, 19), (32, 19), (33, 19), (3, 20), (4, 20), (5, 20), (6, 20), (7, 20), (8, 20), (9, 20), (10, 20), (11, 20), (12, 20), (13, 20), (14, 20), (22, 20), (23, 20), (24, 20), (25, 20), (26, 20), (27, 20), (28, 20), (29, 20), (30, 20), (31, 20), (32, 20), (33, 20), (3, 21), (4, 21), (5, 21), (6, 21), (7, 21), (8, 21), (9, 21), (10, 21), (11, 21), (12, 21), (13, 21), (14, 21), (22, 21), (23, 21), (24, 21), (25, 21), (26, 21), (27, 21), (28, 21), (29, 21), (30, 21), (31, 21), (32, 21), (33, 21), (3, 22), (4, 22), (5, 22), (6, 22), (7, 22), (8, 22), (9, 22), (10, 22), (11, 22), (12, 22), (13, 22), (14, 22), (15, 22), (16, 22), (17, 22), (18, 22), (19, 22), (20, 22), (21, 22), (22, 22), (23, 22), (24, 22), (25, 22), (26, 22), (27, 22), (28, 22), (29, 22), (30, 22), (31, 22), (32, 22), (33, 22), (3, 23), (4, 23), (5, 23), (6, 23), (7, 23), (8, 23), (9, 23), (10, 23), (11, 23), (12, 23), (13, 23), (14, 23), (15, 23), (16, 23), (17, 23), (18, 23), (19, 23), (20, 23), (21, 23), (22, 23), (23, 23), (24, 23), (25, 23), (26, 23), (27, 23), (28, 23), (29, 23), (30, 23), (31, 23), (32, 23), (33, 23), (3, 24), (4, 24), (5, 24), (6, 24), (7, 24), (8, 24), (9, 24), (10, 24), (11, 24), (12, 24), (13, 24), (14, 24), (15, 24), (16, 24), (17, 24), (18, 24), (19, 24), (20, 24), (21, 24), (22, 24), (23, 24), (24, 24), (25, 24), (26, 24), (27, 24), (28, 24), (29, 24), (30, 24), (31, 24), (32, 24), (33, 24), (3, 25), (4, 25), (5, 25), (6, 25), (7, 25), (8, 25), (9, 25), (10, 25), (11, 25), (12, 25), (13, 25), (14, 25), (15, 25), (16, 25), (17, 25), (18, 25), (19, 25), (20, 25), (21, 25), (22, 25), (23, 25), (24, 25), (25, 25), (26, 25), (27, 25), (28, 25), (29, 25), (30, 25), (31, 25), (32, 25), (33, 25), (3, 26), (4, 26), (5, 26), (6, 26), (7, 26), (8, 26), (9, 26), (10, 26), (11, 26), (12, 26), (13, 26), (14, 26), (15, 26), (16, 26), (17, 26), (18, 26), (19, 26), (20, 26), (21, 26), (22, 26), (23, 26), (24, 26), (25, 26), (26, 26), (27, 26), (28, 26), (29, 26), (30, 26), (31, 26), (32, 26), (33, 26), (3, 27), (4, 27), (5, 27), (6, 27), (7, 27), (8, 27), (9, 27), (10, 27), (11, 27), (12, 27), (13, 27), (14, 27), (15, 27), (16, 27), (17, 27), (18, 27), (19, 27), (20, 27), (21, 27), (22, 27), (23, 27), (24, 27), (25, 27), (26, 27), (27, 27), (28, 27), (29, 27), (30, 27), (31, 27), (32, 27), (33, 27), (3, 28), (4, 28), (5, 28), (6, 28), (7, 28), (8, 28), (9, 28), (10, 28), (11, 28), (12, 28), (13, 28), (14, 28), (15, 28), (16, 28), (17, 28), (18, 28), (19, 28), (20, 28), (21, 28), (22, 28), (23, 28), (24, 28), (25, 28), (26, 28), (27, 28), (7, 29), (8, 29), (9, 29), (10, 29), (11, 29), (12, 29), (13, 29), (14, 29), (15, 29), (16, 29), (17, 29), (18, 29), (19, 29), (20, 29), (21, 29), (22, 29), (23, 29), (24, 29), (25, 29), (26, 29), (27, 29), (7, 30), (8, 30), (9, 30), (10, 30), (11, 30), (12, 30), (13, 30), (14, 30), (15, 30), (16, 30), (17, 30), (18, 30), (19, 30), (20, 30), (21, 30), (22, 30), (23, 30), (24, 30), (25, 30), (26, 30), (27, 30), (7, 31), (8, 31), (9, 31), (10, 31), (11, 31), (12, 31), (13, 31), (14, 31), (15, 31), (16, 31), (17, 31), (18, 31), (19, 31), (20, 31), (21, 31), (22, 31), (23, 31), (24, 31), (25, 31), (26, 31), (27, 31), (7, 32), (8, 32), (9, 32), (10, 32), (11, 32), (12, 32), (13, 32), (14, 32), (15, 32), (16, 32), (17, 32), (18, 32), (19, 32), (20, 32), (21, 32), (22, 32), (23, 32), (24, 32), (25, 32), (26, 32), (27, 32), (7, 33), (8, 33), (9, 33), (10, 33), (11, 33), (12, 33), (13, 33), (14, 33), (15, 33), (16, 33), (17, 33), (18, 33), (19, 33), (20, 33), (21, 33), (22, 33), (23, 33), (24, 33), (25, 33) };

            switch (sizeMetric)
            {
                case 16:
                    dataArray = dataArray12;
                    break;
                case 22:
                    dataArray = dataArray18;
                    break;
                case 36:
                    dataArray = dataArray32;
                    break;
            }

            for(int i = 0; i < dataArray.Length; i++)
            {
                int x = dataArray[i].Item1 * pixelLen + (pixelLen/2);
                int y = dataArray[i].Item2 * pixelLen + (pixelLen/2);
                byte[] rawData = image.GetRawData(y, x); //used to be x,y, but y,x seems to actually work.

                if (rawData[0] == black[0] && rawData[1] == black[1] && rawData[2] == black[2])
                {
                    rawRead.Add("11");
                }
                else if (rawData[0] == white[0] && rawData[1] == white[1] && rawData[2] == white[2])
                {
                    rawRead.Add("10");
                }
                else if (rawData[0] == blue[0] && rawData[1] == blue[1] && rawData[2] == blue[2])
                {
                    rawRead.Add("00");
                }
                else if (rawData[0] == brightBlue[0] && rawData[1] == brightBlue[1] && rawData[2] == brightBlue[2])
                {
                    rawRead.Add("00");
                }
                else if (rawData[0] == red[0] && rawData[1] == red[1] && rawData[2] == red[2])
                {
                    rawRead.Add("01");
                }
                else if (rawData[0] == brightRed[0] && rawData[1] == brightRed[1] && rawData[2] == brightRed[2])
                {
                    rawRead.Add("01");
                }
                else if (rawData[0] == yellow[0] && rawData[1] == yellow[1] && rawData[2] == yellow[2])
                {
                    rawRead.Add("10");
                }
                else
                {
                    rawRead.Add("00");
                }
            }

            StringBuilder str = new StringBuilder();

            foreach(string s in rawRead)
            {
                str.Append(s);
            }
            string output = str.ToString();

            if(output != null)
            {
                return new MainWindow().DecodeMain(output, sizeMetric);
            }
            else
            {
                return "botch";
            }
        }
    }
}
