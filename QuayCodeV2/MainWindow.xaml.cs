using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace QuayCodeV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int inputCount;
        public int sizeMetric;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void encodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = txtInput.Text;
            inputCount = input.Length;

            if(inputCount > 0 && inputCount <= 14 )
            {
                sizeMetric = 12;
            }
            else if(inputCount > 14 && inputCount <= 41 )
            {
                sizeMetric = 18;
            }
            else if(inputCount > 41 && inputCount <= 65)
            {
                sizeMetric = 32;
            }
            else
            {
                MessageBox.Show("Lessen the input guy, it's too damn long!");
            }

            //determine data type?

            debug1.Content = "Size Metric: " + sizeMetric.ToString();

            PadText(input);
            DefineHeader(input);
        }

        private void DefineHeader(string input)
        {
            //Determine the data length and send to draw initial header dbits.
        }

        private void PadText(string input)
        {
            //determine length and necessary length for padding to scale up to size of Quay.
            string output = "empty";

            switch (sizeMetric)
            {
                case 12:
                    output = input.PadRight(20, 'x');
                    break;
                case 18:
                    output = input.PadRight(53, 'x');
                    break;
                case 32:
                    output = input.PadRight(195, 'x');
                    break;
                default:
                    //maybe have an error render that this then pursues?
                    break;
            }

            AddRSEncoding(output, input.Length);
        }

        private void AddRSEncoding(string input, int inputCount)
        {
            ReedSolomonEncoding rs = new ReedSolomonEncoding();
            int[] newints = rs.Encode(input, inputCount);
            char[] chars = new char[newints.Length];

            for(int i = 0; i < newints.Length; i++)
            {
                chars[i] = (char)newints[i];
            }

            string output = new string(chars);

            ConvertToBinary(output);

            encodedOut.Text = output;
        }

        private void ConvertToBinary (string input)
        {
            StringBuilder binaryBuilder = new StringBuilder();
            string output = "";

            foreach(char c in input)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
                output = binaryBuilder.ToString();
            }

            debug3.Content = output.Length;
            debug4.Text = output;
            EncodeToPairs(output);
        }

        private void EncodeToPairs(string input)
        {
            List<string> distributedStrings = new List<string>();

            for(int i = 0; i < input.Length/2; i++)
            {
                if(i + 1 < input.Length/2)
                {
                    string twoChars = input.Substring(i, 2);
                    distributedStrings.Add(twoChars);
                }
                else
                {
                    string singleChar = input.Substring(i, 1);
                    distributedStrings.Add(singleChar);
                }
            }
            string[] pairsArray = distributedStrings.ToArray();
            debug2.Content = "Pairs: " + pairsArray.Length.ToString();
            CreateGraphicCode(pairsArray);
        }

        //============================   DECODE   ============================

        private void decodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        //============================   DRAWING   ============================

        WriteableBitmap bitmap;

        private void CreateGraphicCode(string[] pairs)
        {
            //int sF = (int)bitmapImg.Width / sizeMetric;

            sizeMetric += 2;

            int sF = 240 / sizeMetric;

            int width = sizeMetric * sF + sF;
            int height = sizeMetric * sF + sF;

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            byte[] cyanColour = new byte[] { 255, 255, 0, 255 };
            byte[] magentaColour = new byte[] { 255, 0, 255, 255 };
            byte[] yellowColour = new byte[] { 0, 255, 255, 255 };
            byte[] blackColour = new byte[] { 0, 0, 0, 255 };

            for(int i = 0; i < pairs.Length; i++)
            {
                int pairInt;
                int.TryParse(pairs[i], out pairInt);

                (int, int) coord = DefineDataSlots(i, sizeMetric);

                int col = coord.Item1;
                int row = coord.Item2;

                int rectX = col * sF;
                int rectY = row * sF;

                int rectW = sF;
                int rectH = sF;

                int stride = (sF * bitmap.Format.BitsPerPixel + 7) / 8;

                switch (pairInt)
                {
                    case 00:
                        Byte[] cyanData = ColourIndex(cyanColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), cyanData, stride, 0);
                        break;
                    case 01:
                        Byte[] magentaData = ColourIndex(magentaColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), magentaData, stride, 0);
                        break;
                    case 10:
                        Byte[] yellowData = ColourIndex(yellowColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), yellowData, stride, 0);
                        break;
                    case 11:
                        Byte[] blackData = ColourIndex(blackColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), blackData, stride, 0);
                        break;
                }
                bitmap = CreateOverlay(bitmap, sF);
            }
            this.bitmapImg.Source = bitmap;
        }

        private WriteableBitmap CreateOverlay(WriteableBitmap bitmap, int sF)
        {
            //COORDINATE DATA
            (int, int)[] overlay12B = new (int, int)[] { (1, 1), (3, 1), (5, 1), (7, 1), (9, 1), (11, 1), (12, 1), (3, 2), (5, 2), (10, 2), (12, 2), (1, 3), (2, 3), (3, 3), (5, 3), (5, 4), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (1, 7), (1, 9), (1, 11), (1, 12), (2, 12) };
            (int, int)[] overlay12W = new (int, int)[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0), (12, 0), (13,0), (0, 1), (2, 1), (4, 1), (6, 1), (8, 1), (10, 1), (13, 1), (0, 2), (1, 2), (2, 2), (4, 2), (6, 2), (11, 2), (13, 2), (0, 3), (4, 3), (6, 3), (13, 3), (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (6, 4), (13, 4), (0, 5), (6, 5), (13, 5), (0, 6), (1, 6), (2, 6), (3, 6), (4, 6), (5, 6), (6, 6), (13, 6), (0, 7), (13, 7), (0, 8), (1, 8), (13, 8), (0, 9), (13, 9), (0, 10), (1, 10), (13, 10), (0, 11), (13, 11), (0, 12), (13, 12), (0, 13), (1, 13), (2, 13), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (13, 13) };
            (int, int)[] overlay12C = new (int, int)[] { (7, 2), (8, 2), (9, 2) };

            (int, int)[] overlay18B = new (int, int)[] { };
            (int, int)[] overlay18W = new (int, int)[] { };
            (int, int)[] overlay18C = new (int, int)[] { };

            (int, int)[] overlay32B = new (int, int)[] { };
            (int, int)[] overlay32W = new (int, int)[] { };
            (int, int)[] overlay32C = new (int, int)[] { };


            (int, int)[] overlayB = null;
            (int, int)[] overlayW = null;
            (int, int)[] overlayC = null;

            switch (sizeMetric)
            {
                case 14:
                    overlayB = overlay12B;
                    overlayW = overlay12W;
                    overlayC = overlay12C;
                    break;
                case 18:
                    overlayB = overlay18B;
                    overlayW = overlay18W;
                    overlayC = overlay18C;
                    break;
                case 32:
                    overlayB = overlay32B;
                    overlayW = overlay32W;
                    overlayC = overlay32C;
                    break;
            }

            //COLOUR CODES
            byte[] cyanColour = new byte[] { 255, 255, 0, 255 };
            byte[] magentaColour = new byte[] { 255, 0, 255, 255 };
            byte[] yellowColour = new byte[] { 0, 255, 255, 255 };
            byte[] blackColour = new byte[] { 0, 0, 0, 255 };
            byte[] whiteColour = new byte[] { 255, 255, 255, 255 };

            int rectW = sF;
            int rectH = sF;

            int stride = (sF * bitmap.Format.BitsPerPixel + 7) / 8;

            //Overlay Black Layer
            for (int i = 0; i < overlayB.Length; i++)
            {
                int col = overlayB[i].Item1;
                int row = overlayB[i].Item2;

                int rectX = col * sF;
                int rectY = row * sF;

                Byte[] blackData = ColourIndex(blackColour, rectX, rectY, rectW, rectH);
                bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), blackData, stride, 0);
            }
            //Overlay White Layer
            for (int i = 0; i < overlayW.Length; i++)
            {
                int col = overlayW[i].Item1;
                int row = overlayW[i].Item2;

                //int rectX = Math.Max(col * sF - sF, 0);
                //int rectY = Math.Max(row * sF - sF, 0);

                int rectX = col * sF;
                int rectY = row * sF;

                Byte[] whiteData = ColourIndex(whiteColour, rectX, rectY, rectW, rectH);
                bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), whiteData, stride, 0);
            }
            //Overlay Colour Calibration Layer
            for (int i = 0; i < overlayC.Length; i++)
            {
                int col = overlayC[i].Item1;
                int row = overlayC[i].Item2;

                int rectX = col * sF;
                int rectY = row * sF;

                switch (i)
                {
                    case 0:
                        Byte[] cyanData = ColourIndex(cyanColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), cyanData, stride, 0);
                        break;
                    case 1:
                        Byte[] magentaData = ColourIndex(magentaColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), magentaData, stride, 0);
                        break;
                    case 2:
                        Byte[] yellowData = ColourIndex(yellowColour, rectX, rectY, rectW, rectH);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectW, rectH), yellowData, stride, 0);
                        break;
                }
            }

            return bitmap;

        }

        private byte[] ColourIndex(byte[] inputColour, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            byte[] pixelData = new byte[rectWidth * rectHeight * 4];
            int pixelIndex = 0;

            for (int y = rectY; y < rectY + rectHeight; y++)
            {
                for (int x = rectX; x < rectX + rectWidth; x++)
                {
                    if (pixelIndex >= 1600)
                    {
                        break;
                    }

                    pixelData[pixelIndex] = inputColour[0];
                    pixelData[pixelIndex + 1] = inputColour[1];
                    pixelData[pixelIndex + 2] = inputColour[2];
                    pixelData[pixelIndex + 3] = inputColour[3];

                    pixelIndex += 4;
                }
            }
            return pixelData;
        }

        private (int, int) DefineDataSlots(int i, int sizeMetric)
        {
            (int, int)[] coords12 = new (int, int)[] { (7, 4), (8, 4), (9, 4), (10, 4), (11, 4), (12, 4), (12, 5), (11, 5), (10, 5), (9, 5), (8, 5), (7, 5), (7, 6), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (12, 7), (11, 7), (10, 7), (9, 7), (8, 7), (7, 7), (6, 7), (5, 7), (4, 7), (3, 7), (2, 7), (2, 8), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (12, 9), (11, 9), (10, 9), (9, 9), (8, 9), (7, 9), (6, 9), (5, 9), (4, 9), (3, 9), (2, 9), (2, 10), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (12, 11), (11, 11), (10, 11), (9, 11), (8, 11), (7, 11), (6, 11), (5, 11), (4, 11), (3, 11), (2, 11), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12) };
            (int, int)[] coords18 = new (int, int)[] { (6, 1), (6, 2), (7, 2), (7, 1), (8, 1), (8, 2), (9, 2), (9, 1), (10, 1), (10, 2), (11, 2), (11, 1), (12, 1), (12, 2), (12, 3), (12, 4), (11, 4), (11, 3), (10, 3), (10, 4), (9, 4), (9, 3), (8, 3), (8, 4), (7, 4), (7, 3), (6, 3), (6, 4), (6, 5), (7, 5), (7, 6), (6, 6), (5, 6), (4, 6), (3, 6), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (8, 6), (8, 5), (9, 5), (9, 6), (9, 7), (10, 7), (10, 6), (10, 5), (11, 5), (11, 6), (11, 7), (12, 7), (12, 6), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (16, 6), (15, 6), (14, 6), (13, 6), (13, 7), (14, 7), (15, 7), (16, 7), (16, 8), (16, 9), (15, 9), (15, 8), (14, 8), (14, 9), (13, 9), (13, 8), (12, 8), (12, 9), (11, 9), (11, 8), (10, 8), (10, 9), (9, 9), (9, 8), (8, 8), (8, 9), (7, 9), (7, 8), (6, 8), (5, 8), (4, 8), (3, 8), (3, 9), (4, 9), (5, 9), (6, 9), (6, 10), (5, 10), (4, 10), (3, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (7, 10), (8, 10), (8, 11), (9, 11), (9, 10), (10, 10), (10, 11), (11, 11), (11, 10), (12, 10), (12, 11), (13, 11), (13, 10), (14, 10), (14, 11), (15, 11), (15, 10), (16, 10), (16, 11), (16, 12), (16, 13), (15, 13), (15, 12), (14, 12), (14, 13), (13, 13), (13, 12), (12, 12), (12, 13), (11, 13), (11, 12), (10, 12), (10, 13), (9, 13), (9, 12), (8, 12), (8, 13), (7, 13), (7, 12), (6, 12), (6, 13), (5, 13), (5, 12), (4, 12), (3, 12), (3, 13), (4, 13), (4, 14), (4, 15), (4, 16), (5, 16), (5, 15), (5, 14), (6, 14), (6, 15), (6, 16), (7, 16), (7, 15), (7, 14), (8, 14), (8, 15), (8, 16), (9, 16), (9, 15), (9, 14), (10, 14), (10, 15), (10, 16), (11, 16), (11, 15), (11, 14), (12, 14), (12, 15), (12, 16), (13, 16), (13, 15), (13, 14), (14, 14), (14, 15), (14, 16) };
            (int, int)[] coords32 = new (int, int)[] { (1, 1) };

            switch (sizeMetric)
            {
                case 14:
                    return coords12[i];
                case 18:
                    return coords18[i];
                case 32:
                    return coords32[i];
                default:
                    return coords32[i];
            }
        }

        //============================   DOWNLOAD   ============================

        private void dwnldBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadBitmap(bitmap, "C:\\Users\\Ryan\\Desktop\\Software Testing Ground" + "quay" + DateTime.UtcNow.Ticks + ".png");
        }

        private void DownloadBitmap(WriteableBitmap finalBitmap, string filePath)
        {

            if (finalBitmap != null)
            {
                SaveBitmapAsPng(finalBitmap, filePath);

                // Open a SaveFileDialog to allow the user to specify the download location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "quay" + DateTime.UtcNow.Ticks + ".png";
                saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(filePath, saveFileDialog.FileName, true);
                    MessageBox.Show("Bitmap downloaded.");
                }
            }
            else
            {
                MessageBox.Show("Create a bitmap first.");
            }
        }


        private void SaveBitmapAsPng(WriteableBitmap bitmap, string filePath)
        {
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }

            //MessageBox.Show("Bitmap saved as PNG.");
        }
    }
}
