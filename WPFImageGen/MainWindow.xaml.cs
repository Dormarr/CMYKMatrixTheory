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

namespace WPFImageGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string sample = "Test test";
            ConvertToBinary(sample);
        }

        private void SampleRect()
        {
            int width = 300;
            int height = 300;

            WriteableBitmap bitmap = new WriteableBitmap(width, height, 1, 1, PixelFormats.Bgra32, null);
            uint[] pixels = new uint[width * height];

            int red;
            int green;
            int blue;
            int alpha;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int i = width * y + x;

                    red = 0;
                    green = 255 * y / height;
                    blue = 255 * (width - x) / width;
                    alpha = 255;

                    pixels[i] = (uint)((blue << 24) + (green << 16) + (red << 8) + alpha);
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, 2, 2), pixels, width * 4, 0);
            this.MainImage.Source = bitmap;

        }

        private void ConvertToBinary(string input)
        {
            lblText.Content = "Converting to binary.";
            StringBuilder binaryBuilder = new StringBuilder();

            foreach(char c in input)
            {
                string binary = Convert.ToString(c, 2);//.PadLeft(8, '0');
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
            lblText.Content = distributedStrings.ToString();
            string[] pairsArray = distributedStrings.ToArray();
            AssignPairsToRects(pairsArray);
        }

        private void AssignPairsToRects (string[] pairs)
        {
            lblText.Content = "Assigning Pairs to Rects.";
            int width = 200;
            int height = 200;
            double realWidth = this.ActualWidth;
            double realHeight = this.ActualHeight;
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            int[] pixelTest = new int[width * height];
            int red;
            int green;
            int blue;
            int alpha = 255;

            for(int i =0; i < pairs.Length; i++)
            {
                //string pair = pairs[i];
                int pairInt;
                int.TryParse(pairs[i], out pairInt);


                switch (pairInt)
                {
                    case 0:
                        //make cyan rect
                        red = 0;
                        green = 255;
                        blue = 255;
                        pixelTest[i] = (blue << 24) + (green << 16) + (red << 8) + alpha;
                        break;
                    case 1:
                        //make meganta rect
                        red = 255;
                        green = 0;
                        blue = 255;
                        pixelTest[i] = (blue << 24) + (green << 16) + (red << 8) + alpha;
                        break;
                    case 10:
                        //make yellow rect
                        red = 255;
                        green = 255;
                        blue = 0;
                        pixelTest[i] = (blue << 24) + (green << 16) + (red << 8) + alpha;
                        break;
                    case 11:
                        //make black rect
                        red = 0;
                        green = 0;
                        blue = 0;
                        pixelTest[i] = (red << 24) + (green << 16) + (blue << 8) + alpha;
                        break;
                }

                lblText.Content = pixelTest[1].ToString();

                //bitmap.WritePixels(new Int32Rect(0,0,(int)realWidth / 20, (int)realHeight / 20), pixelTest, width * 4, 0);
                bitmap.WritePixels(new Int32Rect(0, 0, 200, 200), pixelTest, width * 4, 0);

            }
            
            this.MainImage.Source = bitmap;
            //lblText.Content = "Finished Rect Assignment.";
        }

        private int PixelValueSwitch(int i)
        {
            int pixelValue;
            switch (i)
            {
                case 1:
                    pixelValue = 20;
                    return pixelValue;
                case 2:
                    pixelValue = 100;
                    return pixelValue;
                case 3: 
                    pixelValue = 175;
                    return pixelValue;
                case 4:
                    pixelValue = 220;
                    return pixelValue;
            }
            return i;
            
            

        }
    }
}
