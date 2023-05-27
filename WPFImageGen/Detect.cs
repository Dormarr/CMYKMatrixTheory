using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Emgu.CV;


namespace WPFImageGen
{
    class Detect
    {
        static public void Main2()
        {

            var vc = new VideoCapture(0, VideoCapture.API.DShow);

            Mat frame = new();
            bool pause = false;

            Mat myface = new();
            Mat templateOutput = new();
            Mat frameGray = new();

            myface = CvInvoke.Imread("C:\\Users\\Ryan\\source\\repos\\CMYKMatrixTheory\\WPFImageGen\\img\\myface.png");
            CvInvoke.CvtColor(myface, myface, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            while (!pause)
            {
                vc.Read(frame);
                /*
                CvInvoke.CvtColor(frame, frameGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                CvInvoke.MatchTemplate(frameGray, myface, templateOutput, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                CvInvoke.Threshold(templateOutput, templateOutput, 0.85, 1, Emgu.CV.CvEnum.ThresholdType.ToZero);

                var matches = templateOutput.ToImage<Gray, byte>();

                for (int i = 0; i < matches.Rows; i++)
                {
                    for (int j = 0; j < matches.Cols; j++)
                    {
                        if (matches[i, j].Intensity > .8)
                        {

                            System.Drawing.Point loc = new System.Drawing.Point(j, i);

                            System.Drawing.Rectangle box = new System.Drawing.Rectangle(loc, myface.Size);

                            CvInvoke.Rectangle(frame, box, new Emgu.CV.Structure.MCvScalar(100, 255, 100), 2);
                        }
                    }
                }

                Image<Bgr, byte> convertFrame = frame.ToImage<Bgr, byte>();
                var image = convertFrame.InRange(new Bgr(75, 0, 0), new Bgr(255, 190, 190));

                for (int i = 0; i < image.Rows; i++)
                {
                    for (int j = 0; j < image.Cols; j++)
                    {
                        var intensity = image[i, j];

                        if (intensity.Intensity > 0)
                        {
                            convertFrame[i, j] = new Bgr(convertFrame[i, j].MCvScalar.V0 - 50, convertFrame[i, j].MCvScalar.V1 - 50, convertFrame[i, j].MCvScalar.V2 + 100);
                        }
                    }
                }

                */
                CvInvoke.Imshow("Show me what you got.", frame);

                int keypressed = CvInvoke.WaitKey(1);
                if (keypressed == 27)
                    pause = true;

            }
        }
        
    }
}

