using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFW;
using OpenGL;
using static OpenGL.GL;

namespace CMYKMatrixTheory
{
    class Program
    {
        static void Main(string[] args)
        {
            //NativeWindow window = new NativeWindow();
            
            string str = "Appleyard";
            string asciiString = PlaintoASCII(str);
            Console.WriteLine();
            PlaintoBinary(str);
            Console.WriteLine();
            ASCIItoBinary(asciiString);



            Console.ReadKey();
        }

        static string PlaintoASCII(string str)
        {
            List<int> combo = new List<int>();
            foreach(char c in str)
            {
                int uni = c;
                //Console.Write("{0} ", uni);
                combo.Add(uni);
            }
            Console.WriteLine(string.Join(" ", combo));
            return combo.ToString();
        }

        static void PlaintoBinary(string str)
        {
            StringBuilder binaryBuilder = new StringBuilder();

            foreach(char c in str)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
            }
            Console.WriteLine(binaryBuilder.ToString());
            
        }

        static void ASCIItoBinary(string str)
        {
            StringBuilder binaryBuilder = new StringBuilder();

            foreach (char c in str)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                binaryBuilder.Append(binary);
            }
            Console.WriteLine(binaryBuilder.ToString());

            //ConvertBinaryToMatrix(binaryBuilder.ToString(), 3, 3);
        }


        static int[,] ConvertBinaryToMatrix(string binaryInput, int rows, int columns)
        {
            int[,] matrix = new int[rows, columns];

            int index = 0;
            for(int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (index >= binaryInput.Length)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        matrix[i, j] = binaryInput[index] == '0' ? 0 : 1;
                    }

                    index++;
                }
            }
            return matrix;
        }
    }
}
