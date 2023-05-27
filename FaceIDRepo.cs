#r "nuget:OpenCvSharp4"
#r "nuget:OpenCvSharp4.Extensions"
#r "nuget:OpenCvSharp4.runtime.win"

using System;
using System.IO;
using OpenCvSharp;

var cascade = new CascadeClassifier(@"../Data/haarcascade_frontalface_alt.xml");
var nestedCascade = new CascadeClassifier(@"../Data/haarcascade_eye.xml");
var color = Scalar.FromRgb(0, 255, 0);

using(VideoCapture capture = new VideoCapture(0))
using(Window window = new Window("Webcam"))
using(Mat srcImage = new Mat())
using(var grayImage = new Mat())
using(var detectedFaceGrayImage = new Mat())
{

    while (capture.IsOpened())
    {
        capture.Read(srcImage);

        Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGRA2GRAY);
        Cv2.EqualizeHist(grayImage, grayImage);

        var faces = cascade.DetectMultiScale(
            image: grayImage,
            minSize: new Size(60, 60)
            );

        foreach (var faceRect in faces)
        {
            using(var detectedFaceImage = new Mat(srcImage, faceRect))
            {
                Cv2.Rectangle(srcImage, faceRect, color, 3);

                Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);
                var nestedObjects = nestedCascade.DetectMultiScale(
                    image: detectedFaceGrayImage,
                    minSize: new Size(30, 30)
                    );

                foreach (var nestedObject in nestedObjects)
                {
                    var center = new Point
                    {
                        X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                        Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                    };
                    var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                    Cv2.Circle(srcImage, center, (int)radius, color, thickness: 2);
                }
            }
        }

        window.ShowImage(srcImage);
        int key = Cv2.WaitKey(1);
        if (key == 27)
        {
            break;
        }                                                                       
    }
}