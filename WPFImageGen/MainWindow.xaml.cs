using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

namespace WPFImageGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap finalBitmap;

        public MainWindow()
        {
            InitializeComponent();
            lblText.Content = "Pogo";
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string sample = txtInput.Text;
            ConvertToBinary(sample);
        }

        private void ConvertToBinary(string input)
        {
            lblText.Content = "Converting to binary.";
            StringBuilder binaryBuilder = new StringBuilder();

            //detect for format. If all numbers then 6 bit.

            int n;
            bool isNumeric = int.TryParse(input, out n);
            string nString = n.ToString();

            foreach (char c in input)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
            }
            


            lblText.Content = binaryBuilder.ToString();
            EncodingToPairs(binaryBuilder.ToString());
        }

        private void EncodingToPairs(string input)
        {
            lblText.Content = "Begun Encoding to Pairs.";
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
                    //lblText.Content = "It Works.";
                    sizeMetric = 18;
                    break;
                case >= 184:
                    //lblText.Content = "Too big a boi.";
                    sizeMetric = 32;
                    break;
                //need to cap
            }

            int width = sizeMetric * 20;
            int height = sizeMetric * 20;
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            int sF = 20; //Scale Factor:: the multiplier for how much bigger we're making this.

            for(int i = 0; i < (sizeMetric * sizeMetric); i++)
            {
                int row = i / sizeMetric;
                int col = i % sizeMetric;

                int rectX = col * sF;
                int rectY = row * sF;

                bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Coral);
            }

            for(int i = 0; i < pairs.Length; i++)
            {

                int pairInt;
                int.TryParse(pairs[i], out pairInt);

                (int, int) coord = DefineDataSlots(i, sizeMetric);

                //This allows wrapping on rows.
                //int row = i / sizeMetric;
                //int col = i % sizeMetric;

                //This does not. Why?
                int row = coord.Item2;// / sizeMetric;
                int col = coord.Item1;// % sizeMetric;

                //lblText.Content = coord.Item1.ToString() + ", " + coord.Item2.ToString();

                int rectX = col * sF;
                int rectY = row * sF;

                //i corresponds to position in coord array.

                switch (pairInt)
                {
                    case 00:
                        bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Cyan);
                        break;
                    case 01:
                        bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Magenta);
                        break;
                    case 10:
                        bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Yellow);
                        break;
                    case 11:
                        bitmap.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.Black);
                        break;
                }
            }

            //merge overlay and data bitmaps.

            //this.OverlayImage.Source = CreateOverlay(sizeMetric);

            this.MainImage.Source = bitmap;
            finalBitmap = bitmap;
        }

        private (int,int) DefineDataSlots(int i, int sizeMetric)
        {
            //Assigned data slot
            (int, int)[] coords12 = new (int, int)[] { (6, 3), (6, 4), (6, 5), (7, 5), (7, 4), (7, 3), (8, 3), (8, 4), (8, 5), (9, 5), (9, 4), (9, 3), (10, 3), (10, 4), (10, 5), (10, 6), (10, 7), (10, 7), (10, 8), (10, 9), (10, 10), (9, 10), (8, 10), (8, 9), (9, 9), (9, 8), (8, 8), (8, 7), (9, 7), (9, 6), (8, 6), (7, 6), (7, 7), (7, 8), (7, 9), (7, 10), (6, 10), (6, 9), (6, 8), (6, 7), (6, 6), (5, 6), (5, 7), (5, 8), (5, 9), (5, 10), (4, 10), (4, 9), (4, 8), (4, 7), (4, 6), (3, 6), (3, 7), (3, 7), (3, 9), (3, 10), (2, 10), (2, 9), (2, 8), (2, 7), (2, 6), (1, 6), (1, 7), (1, 8), (1, 9) };
            (int, int)[] coords18 = new (int, int)[] { (6, 1), (6, 2), (7, 2), (7, 1), (8, 1), (8, 2), (9, 2), (9, 1), (10, 1), (10, 2), (11, 2), (11, 1), (12, 1), (12, 2), (12, 3), (12, 4), (11, 4), (11, 3), (10, 3), (10, 4), (9, 4), (9, 3), (8, 3), (8, 4), (7, 4), (7, 3), (6, 3), (6, 4), (6, 5), (7, 5), (7, 6), (6, 6), (5, 6), (4, 6), (3, 6), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (8, 6), (8, 5), (9, 5), (9, 6), (9, 7), (10, 7), (10, 6), (10, 5), (11, 5), (11, 6), (11, 7), (12, 7), (12, 6), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (16, 6), (15, 6), (14, 6), (13, 6), (13, 7), (14, 7), (15, 7), (16, 7), (16, 8), (16, 9), (15, 9), (15, 8), (14, 8), (14, 9), (13, 9), (13, 8), (12, 8), (12, 9), (11, 9), (11, 8), (10, 8), (10, 9), (9, 9), (9, 8), (8, 8), (8, 9), (7, 9), (7, 8), (6, 8), (5, 8), (4, 8), (3, 8), (3, 9), (4, 9), (5, 9), (6, 9), (6, 10), (5, 10), (4, 10), (3, 10), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (7, 10), (8, 10), (8, 11), (9, 11), (9, 10), (10, 10), (10, 11), (11, 11), (11, 10), (12, 10), (12, 11), (13, 11), (13, 10), (14, 10), (14, 11), (15, 11), (15, 10), (16, 10), (16, 11), (16, 12), (16, 13), (15, 13), (15, 12), (14, 12), (14, 13), (13, 13), (13, 12), (12, 12), (12, 13), (11, 13), (11, 12), (10, 12), (10, 13), (9, 13), (9, 12), (8, 12), (8, 13), (7, 13), (7, 12), (6, 12), (6, 13), (5, 13), (5, 12), (4, 12), (3, 12), (3, 13), (4, 13), (4, 14), (4, 15), (4, 16), (5, 16), (5, 15), (5, 14), (6, 14), (6, 15), (6, 16), (7, 16), (7, 15), (7, 14), (8, 14), (8, 15), (8, 16), (9, 16), (9, 15), (9, 14), (10, 14), (10, 15), (10, 16), (11, 16), (11, 15), (11, 14), (12, 14), (12, 15), (12, 16), (13, 16), (13, 15), (13, 14), (14, 14), (14, 15), (14, 16) };
            (int, int)[] coords32 = new (int, int)[] { (1, 1) };

            lblText.Content = coords18.Length;

            switch (sizeMetric)
            {
                case 12:
                    return coords12[i];
                    break;
                case 18:
                    return coords18[i];
                    break;
                case 32:
                    return coords32[i];
                    break;
                default:
                    return coords32[i];
                    break;
            }

        }

        private WriteableBitmap CreateOverlay(int sizeMetric)
        {
            int width = sizeMetric * 20;
            int height = sizeMetric * 20;
            int sF = 20; //scale factor again.
            WriteableBitmap overlayBMP = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            //Create the overlay graphics for tracking and calibration.
            (int, int)[] overlay12 = new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (0, 5), (0, 6), (0, 7), (0, 8), (0, 9), (0, 10), (0, 11), (1, 0), (2, 0), (3, 0), (4, 0), (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0) };
            (int, int)[] overlay18 = new (int, int)[] { (0, 0) };
            (int, int)[] overlay32 = new (int, int)[] { (0, 0) };



            (int, int)[] overlayFormat = overlay12;
            switch (sizeMetric)
            {
                case 12:
                    overlayFormat = overlay12;
                    break;
                case 18:
                    overlayFormat = overlay18;
                    break;
                case 32:
                    overlayFormat = overlay32;
                    break;
            }

            for(int i = 0; i < overlayFormat.Length; i++)
            {
                int row = i / sizeMetric;
                int col = i % sizeMetric;

                int rectX = col * sF;
                int rectY = row * sF;

                overlayBMP.FillRectangle(rectX, rectY, rectX + sF, rectY + sF, Colors.OrangeRed);
            }
            return overlayBMP;
        }

        private bool CheckAgainstOverlay(int i, int sizeMetric)
        {
            int row = i / sizeMetric;
            int col = i % sizeMetric;

            var coords = new[] { (1, row), (col, 1), (col, 2), (col, 3) };

            for(int j = 0; j < coords.Length; j++)
            {
                if((row, col) == coords[j])
                {
                    //lblText.Content = "Big nice.";
                    return true;
                }
            }
            return false;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            Image<Rgba32> image =  new Image<Rgba32>()
        }

    }
}
