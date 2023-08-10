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
            string output = "empty";

            foreach(char c in input)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
                output = binaryBuilder.ToString();
            }

            EncodeToPairs(output);
        }

        private void EncodeToPairs(string input)
        {
            List<string> distributedStrings = new List<string>();

            for(int i = 0; i < input.Length; i++)
            {
                if(i + i < input.Length)
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
        }

        //============================   DECODE   ============================

        private void decodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
