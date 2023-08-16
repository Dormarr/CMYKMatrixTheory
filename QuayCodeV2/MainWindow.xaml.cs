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
using static System.Net.Mime.MediaTypeNames;

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

        private void drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //============================   ENCODE   ============================

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
            else if(inputCount > 41 && inputCount <= 130)
            {
                sizeMetric = 32;
            }
            else
            {
                MessageBox.Show("Lessen the input guy, it's too damn long!");
                return;
            }

            //determine data type?

            PadText(input);
            GenerateHeader(input);
        }

        private void PadText(string input)
        {
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
                    output = input.PadRight(197, 'x');
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

            string[] toDraw = EncodeToPairs(output);
            CreateGraphicCode(toDraw);
        }

        private string[] EncodeToPairs(string input)
        {
            List<string> distributedStrings = new List<string>();

            for(int i = 0; i < input.Length/2; i++)
            {
                if(i + 1 < input.Length)
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

            return pairsArray;
        }

        private void GenerateHeader(string input)
        {
            //4 Dbits for symbol count. 1 Dbit for data type. 1 Dbit for mask type.

            string symCount;

            if(sizeMetric == 32 || sizeMetric == 34)
            {
                symCount = input.Length.ToString().PadLeft(3, '0'); ;
            }
            else
            {
                symCount = input.Length.ToString().PadLeft(2, '0'); ;
            }

            string dataType = "1"; //elaborate later.
            //string maskType = "4"; //elaborate later.

            string toSend = symCount + dataType;
            char[] prefixChars = toSend.ToCharArray();
            string prefixToSend = CustomBinary.Convert4Bit(prefixChars);

            AdjustHeader(prefixToSend);
        }

        private void AdjustHeader(string prefix)
        {
            String converted;
            switch (sizeMetric)//this is just filler to round out the code.
            {
                case 34:
                    converted = "01100101101011001011";
                    break;
                default:
                    converted = "011101";
                    break;
            }

            string toSend = prefix + converted;

            string[] finalPrefix = EncodeToPairs(toSend);

            DrawHeader(finalPrefix);
        }

        //============================   DECODE   ============================

        private void decodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        //============================   DRAWING   ============================

        WriteableBitmap bitmap;

        byte[] cyanColour = new byte[] { 255, 170, 15, 255 };
        byte[] magentaColour = new byte[] { 140, 45, 255, 255 };
        byte[] yellowColour = new byte[] { 20, 220, 255, 255 };
        byte[] blackColour = new byte[] { 20, 20, 20, 255 };
        byte[] whiteColour = new byte[] { 245, 245, 245, 255 };

        private void CreateGraphicCode(string[] pairs)
        {

            sizeMetric += 2;

            int sF = 240 / sizeMetric;

            int width = sizeMetric * sF;// + sF;
            int height = sizeMetric * sF;// + sF;

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

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

            (int, int)[] overlay18B = new (int, int)[] { (1, 1), (3, 1), (5, 1), (7, 1), (9, 1), (11, 1), (13, 1), (15, 1), (17, 1), (18, 1), (3, 2), (5, 2), (10, 2), (18, 2), (1, 3), (2, 3), (3, 3), (5, 3), (5, 4), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (1, 7), (1, 9), (1, 11), (1, 13), (14, 14), (16, 14), (18, 14), (1, 15), (16, 15), (18, 15), (14, 16), (15, 16), (16, 16), (18, 16), (1, 17), (18, 17), (1, 18), (2, 18), (14, 18), (15, 18), (16, 18), (17, 18), (18, 18) };
            (int, int)[] overlay18W = new (int, int)[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0), (12, 0), (13, 0), (14, 0), (15, 0), (16, 0), (17, 0), (18, 0), (19, 0), (0, 1), (2, 1), (4, 1), (6, 1), (8, 1), (10, 1), (12, 1), (14, 1), (16, 1), (19, 1), (0, 2), (1, 2), (2, 2), (4, 2), (6, 2), (11, 2), (19, 2), (0, 3), (4, 3), (6, 3), (19, 3), (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (6, 4), (19, 4), (0, 5), (6, 5), (19, 5), (0, 6), (1, 6), (2, 6), (3, 6), (4, 6), (5, 6), (6, 6), (19, 6), (0, 7), (19, 7), (0, 8), (1, 8), (19, 8), (0, 9), (19, 9), (0, 10), (1, 10), (19, 10), (0, 11), (19, 11), (0, 12), (1, 12), (19, 12), (0, 13), (13, 13), (14, 13), (15, 13), (16, 13), (17, 13), (18, 13), (19, 13), (0, 14), (1, 14), (13, 14), (15, 14), (17, 14), (19, 14), (0, 15), (13, 15), (14, 15), (15, 15), (17, 15), (19, 15), (0, 16), (1, 16), (13, 16), (17, 16), (19, 16), (0, 17), (13, 17), (14, 17), (15, 17), (16, 17), (17, 17), (19, 17), (0, 18), (13, 18), (19, 18), (0, 19), (1, 19), (2, 19), (3, 19), (4, 19), (5, 19), (6, 19), (7, 19), (8, 19), (9, 19), (10, 19), (11, 19), (12, 19), (13, 19), (14, 19), (15, 19), (16, 19), (17, 19), (18, 19), (19, 19) };
            (int, int)[] overlay18C = new (int, int)[] { (7, 2), (8, 2), (9, 2) };

            (int, int)[] overlay32B = new (int, int)[] { (1, 1), (3, 1), (5, 1), (7, 1), (9, 1), (11, 1), (13, 1), (15, 1), (17, 1), (19, 1), (21, 1), (23, 1), (25, 1), (27, 1), (29, 1), (30, 1), (31, 1), (32, 1), (3, 2), (5, 2), (10, 2), (32, 2), (1, 3), (2, 3), (3, 3), (5, 3), (29, 3), (30, 3), (32, 3), (5, 4), (29, 4), (30, 4), (32, 4), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (1, 7), (1, 9), (1, 11), (1, 13), (1, 15), (15, 15), (16, 15), (17, 15), (18, 15), (19, 15), (15, 16), (19, 16), (1, 17), (15, 17), (17, 17), (19, 17), (15, 18), (19, 18), (1, 19), (15, 19), (16, 19), (17, 19), (18, 19), (19, 19), (1, 21), (1, 23), (1, 25), (1, 27), (28, 28), (30, 28), (32, 28), (1, 29), (3, 29), (4, 29), (30, 29), (32, 29), (1, 30), (3, 30), (4, 30), (28, 30), (29, 30), (30, 30), (32, 30), (1, 31), (32, 31), (1, 32), (2, 32), (3, 32), (4, 32), (28, 32), (29, 32), (30, 32), (31, 32), (32, 32) };
            (int, int)[] overlay32W = new (int, int)[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0), (12, 0), (13, 0), (14, 0), (15, 0), (16, 0), (17, 0), (18, 0), (19, 0), (20, 0), (21, 0), (22, 0), (23, 0), (24, 0), (25, 0), (26, 0), (27, 0), (28, 0), (29, 0), (30, 0), (31, 0), (32, 0), (33, 0), (0, 1), (2, 1), (4, 1), (6, 1), (8, 1), (10, 1), (12, 1), (14, 1), (16, 1), (18, 1), (20, 1), (22, 1), (24, 1), (26, 1), (28, 1), (33, 1), (0, 2), (1, 2), (2, 2), (4, 2), (6, 2), (11, 2), (28, 2), (29, 2), (30, 2), (31, 2), (33, 2), (0, 3), (4, 3), (6, 3), (28, 3), (31, 3), (33, 3), (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (6, 4), (28, 4), (31, 4), (33, 4), (0, 5), (6, 5), (28, 5), (29, 5), (30, 5), (31, 5), (32, 5), (33, 5), (0, 6), (1, 6), (2, 6), (3, 6), (4, 6), (5, 6), (6, 6), (33, 6), (0, 7), (33, 7), (0, 8), (1, 8), (33, 8), (0, 9), (33, 9), (0, 10), (1, 10), (33, 10), (0, 11), (33, 11), (0, 12), (1, 12), (33, 12), (0, 13), (33, 13), (0, 14), (1, 14), (14, 14), (15, 14), (16, 14), (17, 14), (18, 14), (19, 14), (20, 14), (33, 14), (0, 15), (14, 15), (20, 15), (33, 15), (0, 16), (1, 16), (14, 16), (16, 16), (17, 16), (18, 16), (20, 16), (33, 16), (0, 17), (14, 17), (16, 17), (18, 17), (20, 17), (33, 17), (0, 18), (1, 18), (14, 18), (16, 18), (17, 18), (18, 18), (20, 18), (33, 18), (0, 19), (14, 19), (20, 19), (33, 19), (0, 20), (1, 20), (14, 20), (15, 20), (16, 20), (17, 20), (18, 20), (19, 20), (20, 20), (33, 20), (0, 21), (33, 21), (0, 22), (1, 22), (33, 22), (0, 23), (33, 23), (0, 24), (1, 24), (33, 24), (0, 25), (33, 25), (0, 26), (1, 26), (33, 26), (0, 27), (27, 27), (28, 27), (29, 27), (30, 27), (31, 27), (32, 27), (33, 27), (0, 28), (1, 28), (2, 28), (3, 28), (4, 28), (5, 28), (27, 28), (29, 28), (31, 28), (33, 28), (0, 29), (2, 29), (5, 29), (27, 29), (28, 29), (29, 29), (31, 29), (33, 29), (0, 30), (2, 30), (5, 30), (27, 30), (31, 30), (33, 30), (0, 31), (2, 31), (3, 31), (4, 31), (5, 31), (27, 31), (28, 31), (29, 31), (30, 31), (31, 31), (33, 31), (0, 32), (5, 32), (27, 32), (33, 32), (0, 33), (1, 33), (2, 33), (3, 33), (4, 33), (5, 33), (6, 33), (7, 33), (8, 33), (9, 33), (10, 33), (11, 33), (12, 33), (13, 33), (14, 33), (15, 33), (16, 33), (17, 33), (18, 33), (19, 33), (20, 33), (21, 33), (22, 33), (23, 33), (24, 33), (25, 33), (26, 33), (27, 33), (28, 33), (29, 33), (30, 33), (31, 33), (32, 33), (33, 33) };
            (int, int)[] overlay32C = new (int, int)[] { (7, 2), (8, 2), (9, 2) };


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
                case 20:
                    overlayB = overlay18B;
                    overlayW = overlay18W;
                    overlayC = overlay18C;
                    break;
                case 34:
                    overlayB = overlay32B;
                    overlayW = overlay32W;
                    overlayC = overlay32C;
                    break;
                default:
                    overlayB = overlay32B;
                    overlayW = overlay32W;
                    overlayC = overlay32C;
                    break;
            }

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

        private void DrawHeader(string[] prefix)
        {

            int sF = 240 / sizeMetric;

            (int, int)[] prefixSlots12 = new (int, int)[] { (7, 3), (8, 3), (9, 3), (10, 3), (11, 3), (12, 3), (10, 12), (11, 12), (12, 12) };
            (int, int)[] prefixSlots18 = new (int, int)[] { (12, 2), (13, 2), (14, 2), (15, 2), (16, 2), (17, 2), (10, 18), (11, 18), (12, 18) };
            (int, int)[] prefixSlots32 = new (int, int)[] { (12, 2), (13, 2), (14, 2), (15, 2), (16, 2), (17, 2), (18, 2), (19, 2), (20, 2), (21, 2), (22, 2), (23, 2), (24, 2), (25, 2), (26, 2), (27, 2), (25, 32), (26, 32) };

            (int, int)[] prefixSlots = null;

            switch (sizeMetric)
            {
                case 14:
                    prefixSlots = prefixSlots12;
                    break;
                case 20:
                    prefixSlots = prefixSlots18;
                    break;
                case 34:
                    prefixSlots = prefixSlots32;
                    break;

            }

            for (int i = 0; i < prefix.Length; i++)
            {
                int pairInt;
                int.TryParse(prefix[i], out pairInt);

                (int, int) coord = prefixSlots[i];

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
            (int, int)[] coords18 = new (int, int)[] { (7, 3), (8, 3), (9, 3), (10, 3), (11, 3), (12, 3), (13, 3), (14, 3), (15, 3), (16, 3), (17, 3), (18, 3), (7, 4), (8, 4), (9, 4), (10, 4), (11, 4), (12, 4), (13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (7, 5), (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (17, 5), (18, 5), (7, 6), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (14, 6), (15, 6), (16, 6), (17, 6), (18, 6), (2, 7), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (2, 8), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (15, 8), (16, 8), (17, 8), (18, 8), (2, 9), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (14, 9), (15, 9), (16, 9), (17, 9), (18, 9), (2, 10), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (15, 10), (16, 10), (17, 10), (18, 10), (2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (14, 11), (15, 11), (16, 11), (17, 11), (18, 11), (2, 12), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (14, 12), (15, 12), (16, 12), (17, 12), (18, 12), (2, 13), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (2, 14), (3, 14), (4, 14), (5, 14), (6, 14), (7, 14), (8, 14), (9, 14), (10, 14), (11, 14), (12, 14), (2, 15), (3, 15), (4, 15), (5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15), (11, 15), (12, 15), (2, 16), (3, 16), (4, 16), (5, 16), (6, 16), (7, 16), (8, 16), (9, 16), (10, 16), (11, 16), (12, 16), (2, 17), (3, 17), (4, 17), (5, 17), (6, 17), (7, 17), (8, 17), (9, 17), (10, 17), (11, 17), (12, 17), (3, 18), (4, 18), (5, 18), (6, 18), (7, 18), (8, 18), (9, 18), (10, 18), (11, 18), (12, 18) };
            (int, int)[] coords32 = new (int, int)[] { (7, 3), (8, 3), (9, 3), (10, 3), (11, 3), (12, 3), (13, 3), (14, 3), (15, 3), (16, 3), (17, 3), (18, 3), (19, 3), (20, 3), (21, 3), (22, 3), (23, 3), (24, 3), (25, 3), (26, 3), (27, 3), (7, 4), (8, 4), (9, 4), (10, 4), (11, 4), (12, 4), (13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (19, 4), (20, 4), (21, 4), (22, 4), (23, 4), (24, 4), (25, 4), (26, 4), (27, 4), (7, 5), (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (17, 5), (18, 5), (19, 5), (20, 5), (21, 5), (22, 5), (23, 5), (24, 5), (25, 5), (26, 5), (27, 5), (7, 6), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (14, 6), (15, 6), (16, 6), (17, 6), (18, 6), (19, 6), (20, 6), (21, 6), (22, 6), (23, 6), (24, 6), (25, 6), (26, 6), (27, 6), (28, 6), (29, 6), (30, 6), (31, 6), (32, 6), (2, 7), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (19, 7), (20, 7), (21, 7), (22, 7), (23, 7), (24, 7), (25, 7), (26, 7), (27, 7), (28, 7), (29, 7), (30, 7), (31, 7), (32, 7), (2, 8), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (15, 8), (16, 8), (17, 8), (18, 8), (19, 8), (20, 8), (21, 8), (22, 8), (23, 8), (24, 8), (25, 8), (26, 8), (27, 8), (28, 8), (29, 8), (30, 8), (31, 8), (32, 8), (2, 9), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (14, 9), (15, 9), (16, 9), (17, 9), (18, 9), (19, 9), (20, 9), (21, 9), (22, 9), (23, 9), (24, 9), (25, 9), (26, 9), (27, 9), (28, 9), (29, 9), (30, 9), (31, 9), (32, 9), (2, 10), (3, 10), (4, 10), (5, 10), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (15, 10), (16, 10), (17, 10), (18, 10), (19, 10), (20, 10), (21, 10), (22, 10), (23, 10), (24, 10), (25, 10), (26, 10), (27, 10), (28, 10), (29, 10), (30, 10), (31, 10), (32, 10), (2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (14, 11), (15, 11), (16, 11), (17, 11), (18, 11), (19, 11), (20, 11), (21, 11), (22, 11), (23, 11), (24, 11), (25, 11), (26, 11), (27, 11), (28, 11), (29, 11), (30, 11), (31, 11), (32, 11), (2, 12), (3, 12), (4, 12), (5, 12), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (14, 12), (15, 12), (16, 12), (17, 12), (18, 12), (19, 12), (20, 12), (21, 12), (22, 12), (23, 12), (24, 12), (25, 12), (26, 12), (27, 12), (28, 12), (29, 12), (30, 12), (31, 12), (32, 12), (2, 13), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (13, 13), (14, 13), (15, 13), (16, 13), (17, 13), (18, 13), (19, 13), (20, 13), (21, 13), (22, 13), (23, 13), (24, 13), (25, 13), (26, 13), (27, 13), (28, 13), (29, 13), (30, 13), (31, 13), (32, 13), (2, 14), (3, 14), (4, 14), (5, 14), (6, 14), (7, 14), (8, 14), (9, 14), (10, 14), (11, 14), (12, 14), (13, 14), (21, 14), (22, 14), (23, 14), (24, 14), (25, 14), (26, 14), (27, 14), (28, 14), (29, 14), (30, 14), (31, 14), (32, 14), (2, 15), (3, 15), (4, 15), (5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15), (11, 15), (12, 15), (13, 15), (21, 15), (22, 15), (23, 15), (24, 15), (25, 15), (26, 15), (27, 15), (28, 15), (29, 15), (30, 15), (31, 15), (32, 15), (2, 16), (3, 16), (4, 16), (5, 16), (6, 16), (7, 16), (8, 16), (9, 16), (10, 16), (11, 16), (12, 16), (13, 16), (21, 16), (22, 16), (23, 16), (24, 16), (25, 16), (26, 16), (27, 16), (28, 16), (29, 16), (30, 16), (31, 16), (32, 16), (2, 17), (3, 17), (4, 17), (5, 17), (6, 17), (7, 17), (8, 17), (9, 17), (10, 17), (11, 17), (12, 17), (13, 17), (21, 17), (22, 17), (23, 17), (24, 17), (25, 17), (26, 17), (27, 17), (28, 17), (29, 17), (30, 17), (31, 17), (32, 17), (2, 18), (3, 18), (4, 18), (5, 18), (6, 18), (7, 18), (8, 18), (9, 18), (10, 18), (11, 18), (12, 18), (13, 18), (21, 18), (22, 18), (23, 18), (24, 18), (25, 18), (26, 18), (27, 18), (28, 18), (29, 18), (30, 18), (31, 18), (32, 18), (2, 19), (3, 19), (4, 19), (5, 19), (6, 19), (7, 19), (8, 19), (9, 19), (10, 19), (11, 19), (12, 19), (13, 19), (21, 19), (22, 19), (23, 19), (24, 19), (25, 19), (26, 19), (27, 19), (28, 19), (29, 19), (30, 19), (31, 19), (32, 19), (2, 20), (3, 20), (4, 20), (5, 20), (6, 20), (7, 20), (8, 20), (9, 20), (10, 20), (11, 20), (12, 20), (13, 20), (21, 20), (22, 20), (23, 20), (24, 20), (25, 20), (26, 20), (27, 20), (28, 20), (29, 20), (30, 20), (31, 20), (32, 20), (2, 21), (3, 21), (4, 21), (5, 21), (6, 21), (7, 21), (8, 21), (9, 21), (10, 21), (11, 21), (12, 21), (13, 21), (14, 21), (15, 21), (16, 21), (17, 21), (18, 21), (19, 21), (20, 21), (21, 21), (22, 21), (23, 21), (24, 21), (25, 21), (26, 21), (27, 21), (28, 21), (29, 21), (30, 21), (31, 21), (32, 21), (2, 22), (3, 22), (4, 22), (5, 22), (6, 22), (7, 22), (8, 22), (9, 22), (10, 22), (11, 22), (12, 22), (13, 22), (14, 22), (15, 22), (16, 22), (17, 22), (18, 22), (19, 22), (20, 22), (21, 22), (22, 22), (23, 22), (24, 22), (25, 22), (26, 22), (27, 22), (28, 22), (29, 22), (30, 22), (31, 22), (32, 22), (2, 23), (3, 23), (4, 23), (5, 23), (6, 23), (7, 23), (8, 23), (9, 23), (10, 23), (11, 23), (12, 23), (13, 23), (14, 23), (15, 23), (16, 23), (17, 23), (18, 23), (19, 23), (20, 23), (21, 23), (22, 23), (23, 23), (24, 23), (25, 23), (26, 23), (27, 23), (28, 23), (29, 23), (30, 23), (31, 23), (32, 23), (2, 24), (3, 24), (4, 24), (5, 24), (6, 24), (7, 24), (8, 24), (9, 24), (10, 24), (11, 24), (12, 24), (13, 24), (14, 24), (15, 24), (16, 24), (17, 24), (18, 24), (19, 24), (20, 24), (21, 24), (22, 24), (23, 24), (24, 24), (25, 24), (26, 24), (27, 24), (28, 24), (29, 24), (30, 24), (31, 24), (32, 24), (2, 25), (3, 25), (4, 25), (5, 25), (6, 25), (7, 25), (8, 25), (9, 25), (10, 25), (11, 25), (12, 25), (13, 25), (14, 25), (15, 25), (16, 25), (17, 25), (18, 25), (19, 25), (20, 25), (21, 25), (22, 25), (23, 25), (24, 25), (25, 25), (26, 25), (27, 25), (28, 25), (29, 25), (30, 25), (31, 25), (32, 25), (2, 26), (3, 26), (4, 26), (5, 26), (6, 26), (7, 26), (8, 26), (9, 26), (10, 26), (11, 26), (12, 26), (13, 26), (14, 26), (15, 26), (16, 26), (17, 26), (18, 26), (19, 26), (20, 26), (21, 26), (22, 26), (23, 26), (24, 26), (25, 26), (26, 26), (27, 26), (28, 26), (29, 26), (30, 26), (31, 26), (32, 26), (2, 27), (3, 27), (4, 27), (5, 27), (6, 27), (7, 27), (8, 27), (9, 27), (10, 27), (11, 27), (12, 27), (13, 27), (14, 27), (15, 27), (16, 27), (17, 27), (18, 27), (19, 27), (20, 27), (21, 27), (22, 27), (23, 27), (24, 27), (25, 27), (26, 27), (6, 28), (7, 28), (8, 28), (9, 28), (10, 28), (11, 28), (12, 28), (13, 28), (14, 28), (15, 28), (16, 28), (17, 28), (18, 28), (19, 28), (20, 28), (21, 28), (22, 28), (23, 28), (24, 28), (25, 28), (26, 28), (6, 29), (7, 29), (8, 29), (9, 29), (10, 29), (11, 29), (12, 29), (13, 29), (14, 29), (15, 29), (16, 29), (17, 29), (18, 29), (19, 29), (20, 29), (21, 29), (22, 29), (23, 29), (24, 29), (25, 29), (26, 29), (6, 30), (7, 30), (8, 30), (9, 30), (10, 30), (11, 30), (12, 30), (13, 30), (14, 30), (15, 30), (16, 30), (17, 30), (18, 30), (19, 30), (20, 30), (21, 30), (22, 30), (23, 30), (24, 30), (25, 30), (26, 30), (6, 31), (7, 31), (8, 31), (9, 31), (10, 31), (11, 31), (12, 31), (13, 31), (14, 31), (15, 31), (16, 31), (17, 31), (18, 31), (19, 31), (20, 31), (21, 31), (22, 31), (23, 31), (24, 31), (25, 31), (26, 31), (6, 32), (7, 32), (8, 32), (9, 32), (10, 32), (11, 32), (12, 32), (13, 32), (14, 32), (15, 32), (16, 32), (17, 32), (18, 32), (19, 32), (20, 32), (21, 32), (22, 32), (23, 32), (24, 32), (25, 32), (26, 32) };

            switch (sizeMetric)
            {
                case 14:
                    return coords12[i];
                case 20:
                    return coords18[i];
                case 34:
                    return coords32[i];
                default:
                    return coords32[i];
            }
        }

        //============================   DOWNLOAD   ============================

        private void dwnldBtn_Click(object sender, RoutedEventArgs e)
        {
            string downloadsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

            DownloadBitmap(bitmap, downloadsFolderPath + "quay" + DateTime.UtcNow.Ticks + ".png");
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
        }
    }
}
