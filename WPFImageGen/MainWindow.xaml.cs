using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace WPFImageGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();
            lblText.Content = "Pogo";
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string input = txtInput.Text;
            ConvertToBinary(input);
        }

        private void ConvertToBinary(string input)
        {
            lblText.Content = "Converting to binary.";
            StringBuilder binaryBuilder = new StringBuilder();

            //detect for format. If all numbers then 6 bit.

            //string huffString = Convert.ToString(huffBits);

            //int n;
            //bool isNumeric = int.TryParse(huffString, out n);
            //string nString = n.ToString();
            

            if(chkHuffman.IsChecked == true)
            {
                HuffmanTree huffTree = new HuffmanTree();
                huffTree.Build(input);
                BitArray huffBits = huffTree.Encode(input);

                foreach (bool bit in huffBits)
                {
                    binaryBuilder.Append(bit ? "1" : "0");
                }

                string dictString = "";

                foreach(KeyValuePair<char, int> keyVal in huffTree.Freq)
                {
                    dictString += keyVal.Key + "" + keyVal.Value + "";
                }

                StringBuilder dictBinary = new StringBuilder();
                
                foreach(char c in dictString)
                {
                    string binary = Convert.ToString(c, 2);
                    dictBinary.Append(binary);

                }

                lblText.Content = dictBinary.Length.ToString();
                //lblText.Content = dictString;
                //lblText.Content = huffTree.Decode(binaryBuilder.ToString());
                //lblText.Content = binaryBuilder.ToString();
            }
            else
            {
                foreach (char c in input)
                {
                    string binary = Convert.ToString(c, 2);//.PadLeft(8, '0');
                    binaryBuilder.Append(binary);
                    lblText.Content = binaryBuilder.Length.ToString();
                }

            }

            EncodingToPairs(binaryBuilder.ToString());
        }

        private void EncodingToPairs(string input)
        {
            List<string> distributedStrings = new List<string>();

            for (int i = 0; i < input.Length; i += 2)
            {
                if (i + 1 < input.Length)
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
            //AssignPairsToRects(pairsArray);
            CreateCode(pairsArray);
        }

        private void CreateCode(string[] pairs)
        {
            //check amount of pairs. If more than x amount, auto change size of code.
            //Also needs to go through more conditioning before the data can be used.

            int sizeMetric;

            switch (pairs.Length)
            {
                case < 64:
                    sizeMetric = 12;
                    break;
                case >= 64 and < 184:
                    sizeMetric = 18;
                    break;
                case >= 184:
                    sizeMetric = 32;
                    break;
                    //need to cap
            }

            int sF = 20; //Scale Factor:: the multiplier for how much bigger we're making this.
            //reasses sF. Should really be 240/sizeMetric otherwise you just make shit bigger. sF basically = pixel per pixel.
            int width = sizeMetric * sF;
            int height = sizeMetric * sF;
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            for (int i = 0; i < (sizeMetric * sizeMetric); i++)
            {
                int row = i / sizeMetric;
                int col = i % sizeMetric;

                int rectX = col * sF;
                int rectY = row * sF;

                int rectWidth = sF;
                int rectHeight = sF;

                byte[] pixelData = new byte[rectWidth * rectHeight * 4];

                byte blue = 90;
                byte green = 110;
                byte red = 255;
                byte alpha = 0;

                int pixelIndex = 0;

                for (int y = rectY; y < rectY + rectHeight; y++)
                {
                    for (int x = rectX; x < rectX + rectWidth; x++)
                    {
                        pixelData[pixelIndex] = blue;
                        pixelData[pixelIndex + 1] = green;
                        pixelData[pixelIndex + 2] = red;
                        pixelData[pixelIndex + 3] = alpha;

                        pixelIndex += 4;
                    }
                }
                int stride = (rectWidth * bitmap.Format.BitsPerPixel + 7) / 8;
                Int32Rect rect = new Int32Rect(rectX, rectY, rectWidth, rectHeight);
                bitmap.WritePixels(rect, pixelData, stride, 0);

            }

            for (int i = 0; i < pairs.Length; i++)
            {

                int pairInt;
                int.TryParse(pairs[i], out pairInt);

                (int, int) coord = DefineDataSlots(i, sizeMetric);

                int rectWidth = 20;
                int rectHeight = 20;

                int row = coord.Item2;
                int col = coord.Item1;

                int rectX = col * sF;
                int rectY = row * sF;

                //i corresponds to position in coord array.

                byte[] cyanColour = new byte[] { 255, 255, 0, 255 };
                byte[] magentaColour = new byte[] { 255, 0, 255, 255 };
                byte[] yellowColour = new byte[] { 0, 255, 255, 255 };
                byte[] blackColour = new byte[] { 0, 0, 0, 255 };

                int ryStride = (rectWidth * bitmap.Format.BitsPerPixel + 7) / 8;

                switch (pairInt)
                {
                    case 00:
                        //bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Cyan);
                        Byte[] cyanData = ColourIndex(cyanColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), cyanData, ryStride, 0);
                        break;
                    case 01:
                        //bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Magenta);
                        Byte[] magData = ColourIndex(magentaColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), magData, ryStride, 0);
                        break;
                    case 10:
                        //bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Yellow);
                        Byte[] yelData = ColourIndex(yellowColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), yelData, ryStride, 0);
                        break;
                    case 11:
                        //bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Black);
                        Byte[] blackData = ColourIndex(blackColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), blackData, ryStride, 0);
                        break;
                }

                //combine overlay bitmap?
                CreateOverlay(sizeMetric, sF);
            }

            this.MainImage.Source = bitmap;
        }


        private byte[] ColourIndex(byte[] inputColour, int rectX, int rectY, int rectHeight, int rectWidth)
        {
            //How is this even going to work?
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
            //Assigned data slot
            (int, int)[] coords12 = new (int, int)[] { (6, 3), (6, 4), (6, 5), (7, 5), (7, 4), (7, 3), (8, 3), (8, 4), (8, 5), (9, 5), (9, 4), (9, 3), (10, 3), (10, 4), (10, 5), (10, 6), (10, 7), (10, 7), (10, 8), (10, 9), (10, 10), (9, 10), (8, 10), (8, 9), (9, 9), (9, 8), (8, 8), (8, 7), (9, 7), (9, 6), (8, 6), (7, 6), (7, 7), (7, 8), (7, 9), (7, 10), (6, 10), (6, 9), (6, 8), (6, 7), (6, 6), (5, 6), (5, 7), (5, 8), (5, 9), (5, 10), (4, 10), (4, 9), (4, 8), (4, 7), (4, 6), (3, 6), (3, 7), (3, 8), (3, 9), (3, 10), (2, 10), (2, 9), (2, 8), (2, 7), (2, 6), (1, 6), (1, 7), (1, 8), (1, 9) };
            (int, int)[] coords18 = new (int, int)[] { (6, 1), (6, 2), (7, 2), (7, 1), (8, 1), (8, 2), (9, 2), (9, 1), (10, 1), (10, 2), (11, 2), (11, 1), (12, 1), (12, 2), (12, 3), (12, 4), (11, 4), (11, 3), (10, 3), (10, 4), (9, 4), (9, 3), (8, 3), (8, 4), (7, 4), (7, 3), (6, 3), (6, 4), (6, 5), (7, 5), (7, 6), (6, 6), (5, 6), (4, 6), (3, 6), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (8, 6), (8, 5), (9, 5), (9, 6), (9, 7), (10, 7), (10, 6), (10, 5), (11, 5), (11, 6), (11, 7), (12, 7), (12, 6), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (16, 6), (15, 6), (14, 6), (13, 6), (13, 7), (14, 7), (15, 7), (16, 7), (16, 8), (16, 9), (15, 9), (15, 8), (14, 8), (14, 9), (13, 9), (13, 8), (12, 8), (12, 9), (11, 9), (11, 8), (10, 8), (10, 9), (9, 9), (9, 8), (8, 8), (8, 9), (7, 9), (7, 8), (6, 8), (5, 8), (4, 8), (3, 8), (3, 9), (4, 9), (5, 9), (6, 9), (6, 10), (5, 10), (4, 10), (3, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (7, 10), (8, 10), (8, 11), (9, 11), (9, 10), (10, 10), (10, 11), (11, 11), (11, 10), (12, 10), (12, 11), (13, 11), (13, 10), (14, 10), (14, 11), (15, 11), (15, 10), (16, 10), (16, 11), (16, 12), (16, 13), (15, 13), (15, 12), (14, 12), (14, 13), (13, 13), (13, 12), (12, 12), (12, 13), (11, 13), (11, 12), (10, 12), (10, 13), (9, 13), (9, 12), (8, 12), (8, 13), (7, 13), (7, 12), (6, 12), (6, 13), (5, 13), (5, 12), (4, 12), (3, 12), (3, 13), (4, 13), (4, 14), (4, 15), (4, 16), (5, 16), (5, 15), (5, 14), (6, 14), (6, 15), (6, 16), (7, 16), (7, 15), (7, 14), (8, 14), (8, 15), (8, 16), (9, 16), (9, 15), (9, 14), (10, 14), (10, 15), (10, 16), (11, 16), (11, 15), (11, 14), (12, 14), (12, 15), (12, 16), (13, 16), (13, 15), (13, 14), (14, 14), (14, 15), (14, 16) };
            (int, int)[] coords32 = new (int, int)[] { (1, 1) };

            switch (sizeMetric)
            {
                case 12:
                    return coords12[i];
                case 18:
                    return coords18[i];
                case 32:
                    return coords32[i];
                default:
                    return coords32[i];
            }

        }

        private void CreateOverlay(int sizeMetric, int sF)
        {
            //just add pixels to bitmap.

            //Need 4 overlays per size. 1 for white, 1 for black, and 1 for each calibration colour square.
            //Could you do colibration squares as incremental array progressing through colours?
            int width = sizeMetric * sF;
            int height = sizeMetric * sF;

            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            switch (sizeMetric)
            {
                case 12:
                    bitmap = CreateOverlay12(sF, width, height);
                    break;
                case 18:
                    //bitmap = CreateOverlay18(sF, width, height);
                    break;
                case 32:
                    //bitmap = CreateOverlay32(sF, width, height);
                    break;
            }

            this.OverlayImage.Source = bitmap;
        }

        private WriteableBitmap CreateOverlay12(int sF, int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            //COORDINATE DATA
            (int, int)[] overlay12B = new (int, int)[] { (0, 0), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0), (0, 1), (0, 5), (0, 9), (0, 11), (0, 2), (5, 1), (9, 1), (11, 1), (2, 2), (3, 2), (5, 2), (6, 2), (8, 2), (10, 2), (11, 2), (0, 3), (2, 3), (3, 3), (5, 3), (11, 3), (0, 4), (5, 4), (11, 4), (0, 5), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (11, 5), (0, 6), (11, 6), (0, 7), (11, 7), (0, 8), (11, 8), (0, 9), (11, 9), (0, 10), (11, 10), (0, 11), (1, 11), (2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11) };
            (int, int)[] overlay12W = new (int, int)[] { (1, 1), (2, 1), (3, 1), (4, 1), (1, 2), (4, 2), (7, 2), (9, 2), (1, 3), (4, 3), (1, 4), (2, 4), (3, 4), (4, 4) };
            (int, int)[] overlay12C = new (int, int)[] { (6, 1), (7, 1), (8, 1) };

            //COLOUR CODES
            byte[] cyanColour = new byte[] { 255, 255, 0, 255 };
            byte[] magentaColour = new byte[] { 255, 0, 255, 255 };
            byte[] yellowColour = new byte[] { 0, 255, 255, 255 };
            byte[] blackColour = new byte[] { 0, 0, 0, 255 };
            byte[] whiteColour = new byte[] { 255, 255, 255, 255 };

            int rectWidth = width / 12;
            int rectHeight = height / 12;

            int ryStride = (rectWidth * bitmap.Format.BitsPerPixel + 7) / 8;

            //Overlay Black Layer
            for (int i = 0; i < overlay12B.Length; i++)
            {
                int col = overlay12B[i].Item1;
                int row = overlay12B[i].Item2;

                int rectX = col * sF;
                int rectY = row * sF;

                Byte[] blackData = ColourIndex(blackColour, rectX, rectY, rectHeight, rectWidth);
                bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), blackData, ryStride, 0);
            }
            //Overlay White Layer
            for (int i = 0; i < overlay12W.Length; i++)
            {
                int col = overlay12W[i].Item1;
                int row = overlay12W[i].Item2;

                int rectX = col * sF;
                int rectY = row * sF;

                Byte[] whiteData = ColourIndex(whiteColour, rectX, rectY, rectHeight, rectWidth);
                bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), whiteData, ryStride, 0);
            }
            //Overlay Colour Calibration Layer
            for (int i = 0; i < overlay12C.Length; i++)
            {
                int col = overlay12C[i].Item1;
                int row = overlay12C[i].Item2;

                int rectX = col * sF;
                int rectY = row * sF;


                //Add case for white square (10,1) dependent on format, alphanum or numeric.

                switch (i)
                {
                    case 0:
                        Byte[] cyanData = ColourIndex(cyanColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), cyanData, ryStride, 0);
                        break;
                    case 1:
                        Byte[] magentaData = ColourIndex(magentaColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), magentaData, ryStride, 0);
                        break;
                    case 2:
                        Byte[] yellowData = ColourIndex(yellowColour, rectX, rectY, rectHeight, rectWidth);
                        bitmap.WritePixels(new Int32Rect(rectX, rectY, rectWidth, rectHeight), yellowData, ryStride, 0);
                        break;
                }

            }
            return bitmap;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            Detect.Main2();
        }

    }
}

    
